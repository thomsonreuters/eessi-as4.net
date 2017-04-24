using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Http;
using Eu.EDelivery.AS4.UnitTests.Model.Core;
using Microsoft.EntityFrameworkCore;
using SimpleHttpMock;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <see cref="SendAS4MessageStep"/>
    /// </summary>
    public class GivenSendAS4MessageStepFacts : GivenDatastoreFacts
    {
        private static readonly string SharedUrl = UniqueHost.Create();
        private readonly IStep _step;

        /// <summary>
        /// Initializes a new instance of the <see cref="GivenSendAS4MessageStepFacts"/> class.
        /// </summary>
        public GivenSendAS4MessageStepFacts()
        {
            _step = new SendAS4MessageStep(() => new DatastoreContext(Options));

            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        [Fact]
        public async Task StepReturnsStopExecutionResult_IfResponseIsPullRequestError()
        {
            // Arrange
            AS4Message as4Message = new AS4MessageBuilder().WithSignalMessage(new PullRequestError()).Build();

            using (CreateMockedHttpServerThatReturns(as4Message))
            {
                InternalMessage dummyMessage = CreateAnonymousPullRequest();

                // Act
                StepResult actualResult = await _step.ExecuteAsync(dummyMessage, CancellationToken.None);

                // Assert
                Assert.False(actualResult.CanProceed);
            }
        }

        [Theory]
        [InlineData(Constants.ContentTypes.Soap, Operation.Sent, OutStatus.Sent)]
        [InlineData(null, Operation.Undetermined, OutStatus.Exception)]
        public async Task StepUpdatesRequestOperationAndStatus_IfResponseIs(
            string contentType, Operation expectedOperation, OutStatus expectedStatus)
        {
            // Arrange
            InternalMessage stubMessage = CreateAnonymousMessage();
            stubMessage.AS4Message.ContentType = contentType;
            InsertToBeSentUserMessage(stubMessage);

            using (CreateMockedHttpServerThatReturns(CreateAnonymousReceipt()))
            {
                // Act
                await _step.ExecuteAsync(stubMessage, CancellationToken.None);
            }

            // Assert
            AssertSentUserMessage(stubMessage,
                message =>
                {
                    Assert.Equal(expectedOperation, message.Operation);
                    Assert.Equal(expectedStatus, message.Status);
                });
        }

        private void InsertToBeSentUserMessage(InternalMessage requestMessage)
        {
            using (var context = new DatastoreContext(Options))
            {
                context.OutMessages.Add(new OutMessage {EbmsMessageId = requestMessage.AS4Message.PrimaryUserMessage.MessageId});
                context.SaveChanges();
            }
        }

        private static AS4Message CreateAnonymousReceipt()
        {
            return new AS4MessageBuilder().WithSignalMessage(new Receipt()).Build();
        }

        private static MockedHttpServer CreateMockedHttpServerThatReturns(AS4Message as4Message)
        {
            var builder = new MockedHttpServerBuilder();

            builder.WhenPost(SharedUrl)
                   .RespondContent(HttpStatusCode.OK, request => HttpContentFor(as4Message));

            return builder.Build(SharedUrl);
        }

        private static HttpContent HttpContentFor(AS4Message message)
        {
            using (var messageStream = new MemoryStream())
            {
                new SoapEnvelopeSerializer().Serialize(message, messageStream, CancellationToken.None);
                messageStream.Position = 0;
                byte[] messageBytes = messageStream.ToArray();

                return new StringContent(
                    Encoding.UTF8.GetString(messageBytes),
                    Encoding.UTF8,
                    Constants.ContentTypes.Soap);
            }
        }

        private void AssertSentUserMessage(InternalMessage requestMessage, Action<OutMessage> assertion)
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
            InternalMessage dummyMessage = CreateAnonymousPullRequest();
            using (CreateMockedHttpServerThatReturnsStatusCode(HttpStatusCode.Accepted))
            {
                // Act
                StepResult actualResult = await _step.ExecuteAsync(dummyMessage, CancellationToken.None);

                // Assert
                Assert.True(actualResult.InternalMessage.AS4Message.IsEmpty);
            }
        }

        private static MockedHttpServer CreateMockedHttpServerThatReturnsStatusCode(HttpStatusCode statusCode)
        {
            var builder = new MockedHttpServerBuilder();

            builder.WhenPost(SharedUrl)
                .Respond(statusCode);

            return builder.Build(SharedUrl);
        }

        private static InternalMessage CreateAnonymousPullRequest()
        {
            var builder = new AS4MessageBuilder();

            builder.WithSendingPMode(CreateValidSendingPMode()).WithSignalMessage(new PullRequest(mpc: null, messageId: "message-id"));

            return new InternalMessage(builder.Build());
        }

        private static InternalMessage CreateAnonymousMessage()
        {
            var builder = new AS4MessageBuilder();

            builder.WithSendingPMode(CreateValidSendingPMode()).WithUserMessage(new UserMessage(messageId: "message-id")).Build();
            AS4Message as4Message = builder.Build();
            as4Message.ContentType = Constants.ContentTypes.Soap;

            return new InternalMessage(as4Message);
        }

        private static SendingProcessingMode CreateValidSendingPMode()
        {
            return new SendingProcessingMode
            {
                PullConfiguration = new PullConfiguration {Protocol = {Url = SharedUrl}},
                PushConfiguration = new PushConfiguration { Protocol = {Url = SharedUrl}},
                Reliability = {ReceptionAwareness = {IsEnabled = true}}
            };
        }
    }
}