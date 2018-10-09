using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    public class GivenSaveReceivedMessageDatastoreFacts : GivenDatastoreStepFacts
    {
        private readonly InMemoryMessageBodyStore _messageBodyStore = new InMemoryMessageBodyStore();

        public GivenSaveReceivedMessageDatastoreFacts()
        {
            Step = new SaveReceivedMessageStep(StubConfig.Default, GetDataStoreContext, _messageBodyStore);
        }

        protected override void Disposing()
        {
            _messageBodyStore.Dispose();
            base.Disposing();
        }

        /// <summary>
        /// Gets a <see cref="IStep" /> implementation to exercise the datastore.
        /// </summary>
        protected override IStep Step { get; }

        [Property(MaxTest = 20)]
        public Property Saves_Bundled_MessageUnits_As_InMessages(MessagingContextMode mode)
        {
            return Prop.ForAll(
                GenMessageUnits().ToArbitrary(),
                messageUnits =>
                {
                    // Arrange
                    AS4Message fixture = AS4Message.Create(messageUnits);
                    var stub = new ReceivedMessage(Stream.Null, Constants.ContentTypes.Soap);
                    var ctx = new MessagingContext(fixture, stub, mode);

                    // Act
                    Step.ExecuteAsync(ctx).GetAwaiter().GetResult();

                    // Assert
                    IEnumerable<InMessage> inserts = 
                        GetDataStoreContext.GetInMessages(m => fixture.MessageIds.Contains(m.EbmsMessageId));

                    IEnumerable<string> expected = fixture.MessageIds.OrderBy(x => x);
                    IEnumerable<string> actual = inserts.Select(i => i.EbmsMessageId).OrderBy(x => x);
                    Assert.True(
                        expected.SequenceEqual(actual),
                        $"{String.Join(", ", expected)} != {String.Join(", ", actual)}");

                    Assert.All(
                        inserts, m =>
                        {
                            bool pushForNonPullReceive = m.MEP == MessageExchangePattern.Push && mode != MessagingContextMode.PullReceive;
                            bool pullForPullReceive = m.MEP == MessageExchangePattern.Pull && mode == MessagingContextMode.PullReceive;

                            Assert.True(
                                pushForNonPullReceive || pullForPullReceive,
                                mode == MessagingContextMode.PullReceive
                                    ? "MEP Binding should be Pull"
                                    : "MEP Binding should be Push");
                        });
                });
        }

        private static Gen<IList<MessageUnit>> GenMessageUnits()
        {
            return Gen.OneOf(
                Gen.Fresh<MessageUnit>(() => new Receipt($"receipt-{Guid.NewGuid()}")),
                Gen.Fresh<MessageUnit>(() => new UserMessage($"user-{Guid.NewGuid()}")))
                      .NonEmptyListOf();
        }

        [Fact]
        public async Task ThenExecuteStepSucceedsAsync()
        {
            // Arrange
            using (MessagingContext context =
                CreateReceivedMessagingContext(AS4Message.Empty, receivingPMode: null))
            {
                // Act
                StepResult result = await Step.ExecuteAsync(context);

                // Assert
                Assert.NotNull(result);
            }
        }

        [Fact]
        public async Task UserMessage_Gets_Saved_As_Duplicate_When_InMessage_Exists_With_Same_EbmsMessageId()
        {
            // Arrange
            string ebmsMessageId = $"user-{Guid.NewGuid()}";
            GetDataStoreContext.InsertInMessage(new InMessage(ebmsMessageId));

            var user = new UserMessage(ebmsMessageId);
            var context = new MessagingContext(
                AS4Message.Create(user),
                new ReceivedMessage(Stream.Null),
                MessagingContextMode.Receive);

            // Act
            await Step.ExecuteAsync(context);

            // Assert
            InMessage actual = GetDataStoreContext.GetInMessage(
                m => m.EbmsMessageId == ebmsMessageId
                     && m.IsDuplicate);

            Assert.True(actual != null, "Saved UserMessage should be marked as duplicate");
        }

        [Fact]
        public async Task SignalMessage_Gets_Saved_As_Duplicate_When_InMessage_Exists_With_Same_EbmsRefToMessageId()
        {
            // Arrange
            string ebmsMessageId = $"receipt-{Guid.NewGuid()}";
            string ebmsRefToMessageId = $"user-{Guid.NewGuid()}";
            GetDataStoreContext.InsertInMessage(
                new InMessage(ebmsMessageId)
                {
                    EbmsRefToMessageId = ebmsRefToMessageId
                });

            var receipt = new Receipt(ebmsMessageId, ebmsRefToMessageId, DateTimeOffset.Now);
            var context = new MessagingContext(
                AS4Message.Create(receipt),
                new ReceivedMessage(Stream.Null), 
                MessagingContextMode.Receive);

            // Act
            await Step.ExecuteAsync(context);

            // Assert
            InMessage actual = GetDataStoreContext.GetInMessage(
                m => m.EbmsMessageId == ebmsMessageId 
                     && m.EbmsRefToMessageId == ebmsRefToMessageId 
                     && m.IsDuplicate);
                
            Assert.True(actual != null, "Saved Receipt should be marked as duplicate");
        }

        [Fact]
        public async Task During_Saving_The_Deserialized_AS4Message_Is_Used_Instead_Of_Deserializing_Incoming_Stream()
        {
            // Arrange
            var receipt = new Receipt($"reftoid-{Guid.NewGuid()}");
            var ctx = new MessagingContext(
                AS4Message.Create(receipt),
                new ReceivedMessage(Stream.Null, Constants.ContentTypes.Soap),
                MessagingContextMode.Receive);

            // Act
            await Step.ExecuteAsync(ctx);

            // Assert
            GetDataStoreContext.AssertInMessage(receipt.MessageId, Assert.NotNull);
        }

        [Fact]
        public async Task ThenExecuteStepIgnoresPullRequests()
        {
            // Arrange
            var pr = AS4Message.Create(
                new PullRequest(
                    $"pr-mpc-{Guid.NewGuid()}",
                    $"pr-msg-id-{Guid.NewGuid()}"));

            // Act
            using (MessagingContext ctx = 
                CreateReceivedMessagingContext(pr, new ReceivingProcessingMode()))
            {
                await Step.ExecuteAsync(ctx);
            }

            // Assert
            InMessage im = GetDataStoreContext.GetInMessage(
                m => m.EbmsMessageId == pr.GetPrimaryMessageId());
            Assert.Null(im);
        }

        [Fact]
        public async Task ThenExecuteStepSavesBothUserAndReceiptMessage()
        {
            // Arrange
            UserMessage um = CreateUserMessage();
            SignalMessage r = CreateReceipt();
            var as4 = AS4Message.Empty;
            as4.AddMessageUnit(um);
            as4.AddMessageUnit(r);

            // Act
            using (MessagingContext ctx = 
                CreateReceivedMessagingContext(as4, new ReceivingProcessingMode()))
            {
                // Act
                await Step.ExecuteAsync(ctx);
            }

            // Assert
            GetDataStoreContext.AssertInMessage(um.MessageId, Assert.NotNull);
            GetDataStoreContext.AssertInMessage(r.MessageId, Assert.NotNull);
        }

        [Fact]
        public async Task ThenExecuteStepIsTestUserMessage()
        {
            // Arrange
            UserMessage userMessage = CreateUserMessage();
            AS4Message as4Message = AS4Message.Create(userMessage);

            var pmode = new ReceivingProcessingMode();
            pmode.Reliability.DuplicateElimination.IsEnabled = true;

            using (MessagingContext messagingContext = CreateReceivedMessagingContext(as4Message, pmode))
            {
                // Act
                await Step.ExecuteAsync(messagingContext);

                // Assert
                InMessage m = GetUserInMessageForEbmsMessageId(userMessage);
                Assert.Equal(Operation.NotApplicable, m.Operation);
            }
        }

        [Fact]
        public async Task ThenExecuteStepUpdatesDuplicateReceiptMessage()
        {
            // Arrange
            SignalMessage signalMessage = new Receipt((string) "ref-to-message-id");
            signalMessage.IsDuplicate = false;

            using (MessagingContext messagingContext =
                CreateReceivedMessagingContext(AS4Message.Create(signalMessage), null))
            {
                // Act           
                // Execute the step twice.     
                StepResult stepResult = await Step.ExecuteAsync(messagingContext);
                Assert.False(stepResult.MessagingContext.AS4Message.FirstSignalMessage.IsDuplicate);
            }

            using (MessagingContext messagingContext =
                CreateReceivedMessagingContext(AS4Message.Create(signalMessage), null))
            {
                StepResult stepResult = await Step.ExecuteAsync(messagingContext);

                // Assert
                Assert.True(stepResult.MessagingContext.AS4Message.FirstSignalMessage.IsDuplicate);
            }
        }

        [Fact]
        public async Task ThenExecuteStepUpdatesDuplicateUserMessage()
        {
            // Arrange
            UserMessage userMessage = CreateUserMessage();
            InsertDuplicateUserMessage(userMessage);

            var pmode = new ReceivingProcessingMode();
            pmode.Reliability.DuplicateElimination.IsEnabled = true;

            using (MessagingContext context =
                CreateReceivedMessagingContext(AS4Message.Create(userMessage), pmode))
            {
                // Act
                await Step.ExecuteAsync(context);
            }

            // Assert
            InMessage m = GetUserInMessageForEbmsMessageId(userMessage);
            Assert.Equal(Operation.NotApplicable, m.Operation);
        }

        private void InsertDuplicateUserMessage(MessageUnit userMessage)
        {
            GetDataStoreContext.InsertInMessage(new InMessage(ebmsMessageId: userMessage.MessageId));
        }

        private InMessage GetUserInMessageForEbmsMessageId(MessageUnit userMessage)
        {
            InMessage inMessage = GetDataStoreContext
                .GetInMessage(m => m.EbmsMessageId.Equals(userMessage.MessageId));

            Assert.NotNull(inMessage);
            Assert.Equal(MessageType.UserMessage, inMessage.EbmsMessageType);

            return inMessage;
        }

        private static UserMessage CreateUserMessage()
        {
            string userMessageId = Guid.NewGuid().ToString();
            return new UserMessage(userMessageId);
        }

        protected MessagingContext CreateReceivedMessagingContext(AS4Message as4Message, ReceivingProcessingMode receivingPMode)
        {
            var stream = new MemoryStream();

            SerializerProvider
                .Default
                .Get(as4Message.ContentType)
                .Serialize(as4Message, stream, CancellationToken.None);

            stream.Position = 0;

            var ctx = new MessagingContext(
                    new ReceivedMessage(stream, as4Message.ContentType),
                    MessagingContextMode.Receive)
                { ReceivingPMode = receivingPMode };

            ctx.ModifyContext(as4Message);
            return ctx;
        }
    }
}