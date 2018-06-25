using System;
using System.Net;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Http;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Eu.EDelivery.AS4.UnitTests.Model;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <see cref="SendAS4MessageStep"/>
    /// </summary>
    public class GivenSendAS4MessageStepFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task After_Send_Updates_Request_Operation_And_Status_To_Sent_For_Exsiting_SendPMode()
        {
            // Arrange
            string ebmsMessageId = $"user-{Guid.NewGuid()}";
            MessagingContext ctx = SetupMessagingContextWithToBeSentMessage(ebmsMessageId);
            ctx.SendingPMode = CreateSendPModeWithPushUrl();

            // Act 
            IStep sut = CreateSendStepWithResponse(
                StubHttpClient.ThatReturns(AS4Message.Create(new Receipt())));

            await sut.ExecuteAsync(ctx);

            // Assert
            GetDataStoreContext.AssertOutMessage(
                ebmsMessageId,
                message =>
                {
                    Assert.Equal(OutStatus.Sent, message.Status.ToEnum<OutStatus>());
                    Assert.Equal(Operation.Sent, message.Operation.ToEnum<Operation>());
                });
        }

        private MessagingContext SetupMessagingContextWithToBeSentMessage(string ebmsMessageId)
        {
            AS4Message tobeSentMsg = AS4Message.Create(new FilledUserMessage(ebmsMessageId));

            var inserted = new OutMessage(ebmsMessageId: ebmsMessageId);
            GetDataStoreContext.InsertOutMessage(inserted, withReceptionAwareness: false);

            var receivedMessage = new ReceivedMessageEntityMessage(
                inserted,
                tobeSentMsg.ToStream(),
                tobeSentMsg.ContentType);

            var messagingContext = new MessagingContext(receivedMessage, MessagingContextMode.Send);
            messagingContext.ModifyContext(tobeSentMsg);

            return messagingContext;
        }

        [Fact]
        public async Task Send_Results_In_Stop_Execution_If_Response_Is_PullRequest_Warning_For_Exsisting_SendPMode()
        {
            // Arrange
            AS4Message as4Message = AS4Message.Create(new PullRequestError());
            IStep sut = CreateSendStepWithResponse(
                StubHttpClient.ThatReturns(as4Message));

            MessagingContext ctx = CreateMessagingContextWithDefaultPullRequest();
            ctx.SendingPMode = CreateSendPModeWithPushUrl();

            // Act
            StepResult actualResult = await sut.ExecuteAsync(ctx);

            // Assert
            Assert.False(actualResult.CanProceed);
        }

        [Fact]
        public async Task Send_Returns_Empty_Response_For_Empty_Request_For_Existing_SendPMode()
        {
            // Arrange
            IStep sut = CreateSendStepWithResponse(
                StubHttpClient.ThatReturns(HttpStatusCode.Accepted));

            MessagingContext ctx = CreateMessagingContextWithDefaultPullRequest();
            ctx.SendingPMode = CreateSendPModeWithPushUrl();

            // Act
            StepResult actualResult = await sut.ExecuteAsync(ctx);

            // Assert
            Assert.True(actualResult.MessagingContext.AS4Message.IsEmpty);
            Assert.False(actualResult.CanProceed);
        }

        [Fact]
        public async Task Send_Returns_Empty_Response_For_Empty_Request_For_Response_SendPMode()
        {
            // Arrange
            IStep sut = CreateSendStepWithResponse(
                StubHttpClient.ThatReturns(HttpStatusCode.Accepted));

            MessagingContext ctx = CreateMessagingContextWithDefaultPullRequest();
            ctx.SendingPMode = CreateSendPModeWithPushUrl();

            // Act
            StepResult actualResult = await sut.ExecuteAsync(ctx);

            // Assert
            Assert.True(actualResult.MessagingContext.AS4Message.IsEmpty);
            Assert.False(actualResult.CanProceed);
        }

        private static MessagingContext CreateMessagingContextWithDefaultPullRequest()
        {
            var pullRequest = AS4Message.Create(
                new PullRequest(mpc: null, messageId: "message-id"));

            return new MessagingContext(
                new ReceivedMessage(
                    pullRequest.ToStream(), 
                    pullRequest.ContentType),
                MessagingContextMode.Receive);
        }

        private IStep CreateSendStepWithResponse(IHttpClient client)
        {
            return new SendAS4MessageStep(GetDataStoreContext, client);
        }

        private static SendingProcessingMode CreateSendPModeWithPushUrl()
        {
            return new SendingProcessingMode
            {
                PushConfiguration = new PushConfiguration
                {
                    Protocol = { Url = "http://ignored/path" }
                },
                Reliability =
                {
                    ReceptionAwareness = { IsEnabled = true }
                }
            };
        }
    }
}