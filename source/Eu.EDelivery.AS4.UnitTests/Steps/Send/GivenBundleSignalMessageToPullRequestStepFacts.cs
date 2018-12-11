using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using static Eu.EDelivery.AS4.UnitTests.Properties.Resources;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    public class GivenBundleSignalMessageToPullRequestStepFacts : GivenDatastoreFacts
    {
        private readonly InMemoryMessageBodyStore _bodyStore = new InMemoryMessageBodyStore();

        [Fact]
        public async Task Only_Unsigned_Signals_Are_PiggyBacked_For_SendingPModes_Where_Signing_Is_Not_Configured()
        {
            // Arrange
            const string url = "http://localhost:8081/msh";
            string mpc = $"mpc-{Guid.NewGuid()}";

            var user = new UserMessage($"user-{Guid.NewGuid()}", mpc);
            var unsignedReceipt = new Receipt($"receipt-{Guid.NewGuid()}", user.MessageId);
            var signedReceipt = new Receipt($"receipt-{Guid.NewGuid()}", user.MessageId);

            InsertUserMessage(user);
            InsertReceipt(unsignedReceipt, Operation.ToBePiggyBacked, url, signed: false);
            InsertReceipt(signedReceipt, Operation.ToBePiggyBacked, url, signed: true);

            var pr = new PullRequest($"pr-{Guid.NewGuid()}", mpc);
            var ctx = new MessagingContext(AS4Message.Create(pr), MessagingContextMode.PullReceive)
            {
                SendingPMode = new SendingProcessingMode
                {
                    PushConfiguration = new PushConfiguration { Protocol = { Url = url } },
                    Security = { Signing = { IsEnabled = false } }
                }
            };
            var sut = new BundleSignalMessageToPullRequestStep(GetDataStoreContext, _bodyStore);

            // Act
            StepResult result = await sut.ExecuteAsync(ctx);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Collection(
                result.MessagingContext.AS4Message.MessageUnits,
                u => Assert.IsType<PullRequest>(u),
                u => Assert.Equal(unsignedReceipt.MessageId, Assert.IsType<Receipt>(u).MessageId));

            GetDataStoreContext.AssertOutMessage(
                signedReceipt.MessageId,
                m => Assert.Equal(Operation.ToBePiggyBacked, m.Operation));

            GetDataStoreContext.AssertOutMessage(
                unsignedReceipt.MessageId,
                m => Assert.Equal(Operation.Sending, m.Operation));
        }

        [Property(MaxTest = 150)]
        public Property Bundle_Receipt_With_PullRequest()
        {
            Gen<Operation> genOperation =
                Gen.Frequency(
                    Tuple.Create(4, Gen.Constant(Operation.ToBePiggyBacked)),
                    Tuple.Create(1, Arb.Generate<Operation>()));

            return Prop.ForAll(
                genOperation.ToArbitrary(),
                EqualAndDiffer(() => $"mpc-{Guid.NewGuid()}").ToArbitrary(),
                EqualAndDiffer(() => $"http://localhost/{Guid.NewGuid()}").ToArbitrary(),
                (operation, urls, mpcs) =>
                {
                    // Arrange
                    var user = new UserMessage($"user-{Guid.NewGuid()}", mpcs.Item1);
                    var receipt = new Receipt($"receipt-{Guid.NewGuid()}", user.MessageId);

                    InsertUserMessage(user);
                    InsertReceipt(receipt, operation, urls.Item1);

                    var pr = new PullRequest($"pr-{Guid.NewGuid()}", mpcs.Item2);

                    // Act
                    StepResult result = ExerciseBundleWithPullRequest(pr, urls.Item2);

                    // Assert
                    AS4Message bundled = result.MessagingContext.AS4Message;

                    bool bundledWithReceipt =
                        bundled.MessageIds.SequenceEqual(new[] { pr.MessageId, receipt.MessageId })
                        && IsPullRequestBundledWithOneReceipt(result);

                    bool isOperationPiggyBacked = operation == Operation.ToBePiggyBacked;
                    bool isMatchedByMpc = mpcs.Item1 == mpcs.Item2;
                    bool isMatchedByUrl = urls.Item1 == urls.Item2;

                    bool operationBecomesSending = GetDataStoreContext
                        .GetInMessages(m => m.EbmsMessageId == receipt.MessageId)
                        .All(m => m.Operation == Operation.Sending);

                    return (isOperationPiggyBacked && isMatchedByMpc && isMatchedByUrl)
                        .Equals(bundledWithReceipt && operationBecomesSending)
                        .Label(
                            "PullRequest isn't bundled with Receipt when the Operation of the "
                            + $"stored Receipt is {operation} and the MPC of the "
                            + $"UserMessage {(isMatchedByMpc ? "matches" : "differs")} from the PullRequest MPC "
                            + $"and Receipt {(isMatchedByUrl ? "matches" : "differs")} from the PullRequest Url");
                });
        }

        private static Gen<Tuple<string, string>> EqualAndDiffer(Func<string> constant)
        {
            return Gen.OneOf(
                Gen.Fresh(constant).Two(),
                Gen.Fresh(constant).Select(mpc => Tuple.Create(mpc, mpc)));
        }

        private static bool IsPullRequestBundledWithOneReceipt(StepResult result)
        {
            IEnumerable<Type> expectedTypes = 
                new[] { typeof(PullRequest) }.Concat(Enumerable.Repeat(typeof(Receipt), 1));

            return result.MessagingContext
                         .AS4Message
                         .MessageUnits
                         .Select(x => x.GetType())
                         .SequenceEqual(expectedTypes);
        }

        private StepResult ExerciseBundleWithPullRequest(PullRequest pullRequest, string url)
        {
            var sut = new BundleSignalMessageToPullRequestStep(GetDataStoreContext, _bodyStore);

            var ctx = new MessagingContext(
                AS4Message.Create(pullRequest),
                MessagingContextMode.PullReceive)
                {
                    SendingPMode = new SendingProcessingMode
                    {
                        PushConfiguration = new PushConfiguration { Protocol = { Url = url } }
                    }
                };

            return sut.ExecuteAsync(ctx)
                      .GetAwaiter()
                      .GetResult();
        }

        private void InsertUserMessage(UserMessage user)
        {
            GetDataStoreContext.InsertInMessage(
                new InMessage(user.MessageId)
                {
                    Mpc = user.Mpc,
                    EbmsMessageType = MessageType.UserMessage,
                    ContentType = Constants.ContentTypes.Soap
                });
        }

        private void InsertReceipt(
            Receipt receipt, 
            Operation operation, 
            string url,
            bool signed = false)
        {
            GetDataStoreContext.InsertOutMessage(
                new OutMessage(receipt.MessageId)
                {
                    EbmsRefToMessageId = receipt.RefToMessageId,
                    EbmsMessageType = MessageType.Receipt,
                    ContentType = Constants.ContentTypes.Soap,
                    MessageLocation = SaveAS4MessageUnit(receipt, signed),
                    Operation = operation,
                    Url = url
                });
        }

        private string SaveAS4MessageUnit(MessageUnit unit, bool signed)
        {
            AS4Message as4Message = AS4Message.Create(unit);

            if (signed)
            {
                var config = new CalculateSignatureConfig(
                    new X509Certificate2(
                        holodeck_partya_certificate, 
                        certificate_password, 
                        X509KeyStorageFlags.Exportable));

                as4Message.Sign(config);
            }

            return _bodyStore.SaveAS4Message("not used location", as4Message);
        }

        protected override void Disposing()
        {
            _bodyStore.Dispose();
        }
    }
}
