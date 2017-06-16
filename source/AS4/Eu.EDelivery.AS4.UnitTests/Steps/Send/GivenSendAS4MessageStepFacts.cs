using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Http;
using Eu.EDelivery.AS4.UnitTests.Model.Core;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <see cref="SendAS4MessageStep"/>
    /// </summary>
    public class GivenSendAS4MessageStepFacts : GivenDatastoreFacts
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GivenSendAS4MessageStepFacts"/> class.
        /// </summary>
        public GivenSendAS4MessageStepFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        [Fact]
        public async Task StepReturnsStopExecutionResult_IfResponseIsPullRequestError()
        {
            // Arrange
            AS4Message as4Message = AS4Message.Create(new PullRequestError());
            var step = new SendAS4MessageStep(GetDataStoreContext, StubHttpClient.ThatReturns(as4Message));

            MessagingContext dummyMessage = CreateAnonymousPullRequest();

            // Act
            StepResult actualResult = await step.ExecuteAsync(dummyMessage, CancellationToken.None);

            // Assert
            Assert.False(actualResult.CanProceed);
        }

        [Theory]
        [InlineData(Constants.ContentTypes.Soap, Operation.Sent, OutStatus.Sent)]
        [InlineData(null, Operation.Undetermined, OutStatus.Exception)]
        public async Task StepUpdatesRequestOperationAndStatus_IfResponseIs(
            string contentType,
            Operation expectedOperation,
            OutStatus expectedStatus)
        {
            // Arrange
            MessagingContext stubMessage = CreateAnonymousMessage();
            stubMessage.AS4Message.ContentType = contentType;
            InsertToBeSentUserMessage(stubMessage);

            var step = new SendAS4MessageStep(GetDataStoreContext, StubHttpClient.ThatReturns(CreateAnonymousReceipt()));

            // Act
            await step.ExecuteAsync(stubMessage, CancellationToken.None);

            // Assert
            AssertSentUserMessage(
                stubMessage,
                message =>
                {
                    Assert.Equal(expectedOperation, message.Operation);
                    Assert.Equal(expectedStatus, message.Status);
                });
        }

        private void InsertToBeSentUserMessage(MessagingContext requestMessage)
        {
            using (var context = new DatastoreContext(Options))
            {
                context.OutMessages.Add(new OutMessage {EbmsMessageId = requestMessage.AS4Message.PrimaryUserMessage.MessageId});
                context.SaveChanges();
            }
        }

        private static AS4Message CreateAnonymousReceipt()
        {
            return AS4Message.Create(new Receipt());
        }

        private void AssertSentUserMessage(MessagingContext requestMessage, Action<OutMessage> assertion)
        {
            using (var context = new DatastoreContext(Options))
            {
                OutMessage outMessage = context.OutMessages.FirstOrDefault(
                    m => m.EbmsMessageId.Equals(requestMessage.AS4Message.PrimaryUserMessage.MessageId));

                assertion(outMessage);
            }
        }

        [Fact]
        public async Task SendReturnsEmptyResponseForEmptyRequest()
        {
            // Arrange
            var step = new SendAS4MessageStep(GetDataStoreContext, StubHttpClient.ThatReturns(HttpStatusCode.Accepted));
            MessagingContext dummyMessage = CreateAnonymousPullRequest();

            // Act
            StepResult actualResult = await step.ExecuteAsync(dummyMessage, CancellationToken.None);

            // Assert
            Assert.True(actualResult.MessagingContext.AS4Message.IsEmpty);
            Assert.False(actualResult.CanProceed);
        }

        private static MessagingContext CreateAnonymousPullRequest()
        {
            return ContextWith(AS4Message.Create(new PullRequest(mpc: null, messageId: "message-id")));
        }

        private static MessagingContext CreateAnonymousMessage()
        {
            return ContextWith(AS4Message.Create(new UserMessage(messageId: "message-id")));
        }

        public static MessagingContext ContextWith(AS4Message message)
        {
            return new MessagingContext(message, MessagingContextMode.Unknown) {SendingPMode = CreateValidSendingPMode()};
        }

        private static SendingProcessingMode CreateValidSendingPMode()
        {
            return new SendingProcessingMode
            {
                PullConfiguration = new PullConfiguration {Protocol = {Url = "http://ignored/path"}},
                PushConfiguration = new PushConfiguration { Protocol = {Url = "http://ignored/path"}},
                Reliability = {ReceptionAwareness = {IsEnabled = true}}
            };
        }
    }
}