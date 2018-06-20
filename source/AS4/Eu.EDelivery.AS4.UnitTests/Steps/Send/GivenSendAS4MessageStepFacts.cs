using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
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
        [Fact]
        public async Task StepUpdatesRequestOperationAndStatus_IfRequestIsBeingSent()
        {
            // Arrange
            var messagingContext = SetupMessagingContextWithToBeSentMessage("some-message-id");

            long outMessageId = ((ReceivedEntityMessage)messagingContext.ReceivedMessage).Entity.Id;

            // Act 
            var step = new SendAS4MessageStep(GetDataStoreContext,
                                              StubHttpClient.ThatReturns(AS4Message.Create(new Receipt())));

            await step.ExecuteAsync(messagingContext);

            // Assert
            AssertSentUserMessage(
                outMessageId,
                message =>
                {
                    Assert.Equal(Operation.Sent, OperationUtils.Parse(message.Operation));
                    Assert.Equal(OutStatus.Sent, message.Status.ToEnum<OutStatus>());
                });
        }

        private MessagingContext SetupMessagingContextWithToBeSentMessage(string ebmsMessageId)
        {
            var as4Message = AS4Message.Create(new FilledUserMessage(ebmsMessageId));

            OutMessage outMessage = null;

            using (var context = GetDataStoreContext())
            {
                outMessage = new OutMessage(ebmsMessageId: ebmsMessageId);
                context.OutMessages.Add(outMessage);
                context.SaveChanges();
            }

            var receivedMessage = new ReceivedMessageEntityMessage(outMessage,
                                                                   as4Message.ToStream(),
                                                                   as4Message.ContentType);

            var messagingContext = new MessagingContext(receivedMessage, MessagingContextMode.Send)
            {
                SendingPMode = CreateValidSendingPMode()
            };

            messagingContext.ModifyContext(as4Message);

            return messagingContext;
        }

        private void AssertSentUserMessage(long outMessageId, Action<OutMessage> assertion)
        {
            using (var context = GetDataStoreContext())
            {
                OutMessage outMessage = context.OutMessages.FirstOrDefault(
                    m => m.Id == outMessageId);

                assertion(outMessage);
            }
        }

        [Fact]
        public async Task StepReturnsStopExecutionResult_IfResponseIsPullRequestError()
        {
            // Arrange
            AS4Message as4Message = AS4Message.Create(new PullRequestError());
            var step = new SendAS4MessageStep(GetDataStoreContext, StubHttpClient.ThatReturns(as4Message));

            MessagingContext dummyMessage = CreateMessagingContextWithDefaultPullRequest();

            // Act
            StepResult actualResult = await step.ExecuteAsync(dummyMessage);

            // Assert
            Assert.False(actualResult.CanProceed);
        }

        [Fact]
        public async Task SendReturnsEmptyResponseForEmptyRequest()
        {
            // Arrange
            var step = new SendAS4MessageStep(GetDataStoreContext, StubHttpClient.ThatReturns(HttpStatusCode.Accepted));
            MessagingContext dummyMessage = CreateMessagingContextWithDefaultPullRequest();

            // Act
            StepResult actualResult = await step.ExecuteAsync(dummyMessage);

            // Assert
            Assert.True(actualResult.MessagingContext.AS4Message.IsEmpty);
            Assert.False(actualResult.CanProceed);
        }

        private static MessagingContext CreateMessagingContextWithDefaultPullRequest()
        {
            var pullRequest = AS4Message.Create(new PullRequest(mpc: null, messageId: "message-id"));

            var ms = pullRequest.ToStream();

            MessagingContext context = new MessagingContext(new ReceivedMessage(ms, pullRequest.ContentType), MessagingContextMode.Receive);
            context.SendingPMode = CreateValidSendingPMode();

            return context;
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