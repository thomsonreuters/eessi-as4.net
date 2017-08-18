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
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Eu.EDelivery.AS4.UnitTests.Model;
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

            MessagingContext dummyMessage = CreateDefaultPullRequest();

            // Act
            StepResult actualResult = await step.ExecuteAsync(dummyMessage, CancellationToken.None);

            // Assert
            Assert.False(actualResult.CanProceed);
        }
        
        [Fact]
        public async Task StepUpdatesRequestOperationAndStatus_IfRequestIsBeingSent()
        {
            // Arrange
            var as4Message = AS4Message.Create(new FilledUserMessage());
            InsertToBeSentUserMessage(as4Message);
            MessagingContext context = CreateSendMessagingContext(as4Message);

            // Act 
            var step = new SendAS4MessageStep(GetDataStoreContext, StubHttpClient.ThatReturns(CreateAnonymousReceipt()));
            await step.ExecuteAsync(context, CancellationToken.None);

            // Assert
            AssertSentUserMessage(
                as4Message,
                message =>
                {
                    Assert.Equal(Operation.Sent, message.Operation);
                    Assert.Equal(OutStatus.Sent, message.Status);
                });
        }

        private void InsertToBeSentUserMessage(AS4Message as4Message)
        {
            using (var context = new DatastoreContext(Options))
            {
                context.OutMessages.Add(new OutMessage { EbmsMessageId = as4Message.PrimaryUserMessage.MessageId });
                context.SaveChanges();
            }
        }

        private static AS4Message CreateAnonymousReceipt()
        {
            return AS4Message.Create(new Receipt());
        }

        private void AssertSentUserMessage(AS4Message as4Message, Action<OutMessage> assertion)
        {
            using (var context = new DatastoreContext(Options))
            {
                OutMessage outMessage = context.OutMessages.FirstOrDefault(
                    m => m.EbmsMessageId.Equals(as4Message.PrimaryUserMessage.MessageId));

                assertion(outMessage);
            }
        }

        [Fact]
        public async Task SendReturnsEmptyResponseForEmptyRequest()
        {
            // Arrange
            var step = new SendAS4MessageStep(GetDataStoreContext, StubHttpClient.ThatReturns(HttpStatusCode.Accepted));
            MessagingContext dummyMessage = CreateDefaultPullRequest();

            // Act
            StepResult actualResult = await step.ExecuteAsync(dummyMessage, CancellationToken.None);

            // Assert
            Assert.True(actualResult.MessagingContext.AS4Message.IsEmpty);
            Assert.False(actualResult.CanProceed);
        }

        private static MessagingContext CreateDefaultPullRequest()
        {
            var pullRequest = AS4Message.Create(new PullRequest(mpc: null, messageId: "message-id"));

            var ms = pullRequest.ToStream();

            MessagingContext context = new MessagingContext(new ReceivedMessage(ms, pullRequest.ContentType), MessagingContextMode.Receive);
            context.SendingPMode = CreateValidSendingPMode();

            return context;
        }

        public static MessagingContext CreateSendMessagingContext(AS4Message message)
        {
            var receivedMessage = new ReceivedMessage(message.ToStream(), message.ContentType);

            return new MessagingContext(receivedMessage, MessagingContextMode.Send) { SendingPMode = CreateValidSendingPMode() };
        }

        private static SendingProcessingMode CreateValidSendingPMode()
        {
            return new SendingProcessingMode
            {                
                PushConfiguration = new PushConfiguration { Protocol = { Url = "http://ignored/path" } },
                Reliability = { ReceptionAwareness = { IsEnabled = true } }
            };
        }
    }
}