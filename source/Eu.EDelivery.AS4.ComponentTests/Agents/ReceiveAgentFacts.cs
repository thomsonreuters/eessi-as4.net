using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Xunit;
using static Eu.EDelivery.AS4.ComponentTests.Properties.Resources;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class ReceiveAgentFacts : ComponentTestTemplate
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        private readonly AS4Component _as4Msh;
        private readonly DatabaseSpy _databaseSpy;
        private readonly string _receiveAgentUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveAgentFacts" /> class.
        /// </summary>
        public ReceiveAgentFacts()
        {
            OverrideSettings("receiveagent_http_settings.xml");

            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);

            _databaseSpy = new DatabaseSpy(_as4Msh.GetConfiguration());

            SettingsAgent receivingAgent =
                _as4Msh.GetConfiguration().GetSettingsAgents().FirstOrDefault(a => a.Name.Equals("Receive Agent"));

            Assert.True(receivingAgent != null, "The Agent with name Receive Agent could not be found");

            _receiveAgentUrl = receivingAgent.Receiver?.Setting?.FirstOrDefault(s => s.Key == "Url")?.Value;

            Assert.False(
                string.IsNullOrWhiteSpace(_receiveAgentUrl),
                "The URL where the receive agent is listening on, could not be retrieved.");
        }

        public class GivenValidReceivedUserMessageFacts : ReceiveAgentFacts
        {
            [Fact]
            public async Task ThenAgentReturnsError_IfMessageHasNonExsistingAttachment()
            {
                // Arrange
                byte[] content = receiveagent_message_nonexist_attachment;

                // Act
                HttpResponseMessage response = await HttpClient.SendAsync(CreateSendAS4Message(content));

                // Assert
                AS4Message as4Message = await DeserializeToAS4Message(response);
                Assert.True(as4Message.IsSignalMessage);
                Assert.True(as4Message.PrimarySignalMessage is Error);
            }

            [Fact]
            public async Task ThenInMessageOperationIsToBeDelivered()
            {
                // Arrange
                byte[] content = receiveagent_message;

                // Act
                HttpResponseMessage response = await HttpClient.SendAsync(CreateSendAS4Message(content));

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                AS4Message receivedAS4Message = await DeserializeToAS4Message(response);
                Assert.True(receivedAS4Message.IsSignalMessage);
                Assert.True(receivedAS4Message.PrimarySignalMessage is Receipt);

                InMessage receivedUserMessage = GetInsertedUserMessageFor(receivedAS4Message);
                Assert.NotNull(receivedUserMessage);
                Assert.Equal(Operation.ToBeDelivered, receivedUserMessage.Operation);
            }

            private HttpRequestMessage CreateSendAS4Message(byte[] content)
            {
                var message = new HttpRequestMessage(HttpMethod.Post, _receiveAgentUrl)
                {
                    Content = new ByteArrayContent(content)
                };

                message.Content.Headers.Add(
                    "Content-Type",
                    "multipart/related; boundary=\"=-C3oBZDXCy4W2LpjPUhC4rw==\"; type=\"application/soap+xml\"; charset=\"utf-8\"");

                return message;
            }

            private static async Task<AS4Message> DeserializeToAS4Message(HttpResponseMessage response)
            {
                ISerializer serializer = SerializerProvider.Default.Get(response.Content.Headers.ContentType.MediaType);

                return await serializer.DeserializeAsync(
                           await response.Content.ReadAsStreamAsync(),
                           response.Content.Headers.ContentType.MediaType,
                           CancellationToken.None);
            }

            private InMessage GetInsertedUserMessageFor(AS4Message receivedAS4Message)
            {
                return
                    _databaseSpy.GetInMessageFor(
                        i => i.EbmsMessageId.Equals(receivedAS4Message.PrimarySignalMessage.RefToMessageId));
            }
        }

        public class GivenValidReceivedSignalMessageFacts : ReceiveAgentFacts
        {
            [Fact]
            public async Task ThenRelatedUserMessageIsAcked()
            {
                // Arrange
                const string expectedId = "message-id";
                CreateExistingOutMessage(expectedId);

                AS4Message as4Message = CreateAS4ReceiptMessage(expectedId);

                // Act
                await HttpClient.SendAsync(CreateSendMessage(as4Message));

                // Assert
                AssertIfStatusOfOutMessageIs(expectedId, OutStatus.Ack);
                AssertIfInMessageExistsForSignalMessage(expectedId);
            }

            private static AS4Message CreateAS4ReceiptMessage(string refToMessageId)
            {
                var r = new Receipt {RefToMessageId = refToMessageId};

                return AS4Message.Create(r, GetSendingPMode());
            }

            [Fact]
            public async Task ThenRelatedUserMessageIsNotAcked()
            {
                // Arrange
                const string expectedId = "message-id";
                CreateExistingOutMessage(expectedId);

                AS4Message as4Message = CreateAS4ErrorMessage(expectedId);

                // Act
                await HttpClient.SendAsync(CreateSendMessage(as4Message));

                // Assert
                AssertIfStatusOfOutMessageIs(expectedId, OutStatus.Nack);
                AssertIfInMessageExistsForSignalMessage(expectedId);
            }

            private void CreateExistingOutMessage(string messageId)
            {
                var outMessage = new OutMessage
                {
                    EbmsMessageId = messageId,
                    Status = OutStatus.Sent,
                    PMode = AS4XmlSerializer.ToString(GetSendingPMode())
                };

                _databaseSpy.InsertOutMessage(outMessage);
            }

            private static SendingProcessingMode GetSendingPMode()
            {
                return new SendingProcessingMode
                {
                    Id = "receive_agent_facts_pmode",
                    ReceiptHandling = { NotifyMessageProducer = true },
                    ErrorHandling = { NotifyMessageProducer = true }
                };
            }

            private static AS4Message CreateAS4ErrorMessage(string refToMessageId)
            {
                AS4Exception exception =
                    AS4ExceptionBuilder.WithDescription("An error occurred")
                                       .WithMessageIds(refToMessageId)
                                       .WithErrorCode(ErrorCode.Ebms0010)
                                       .Build();

                Error error = new ErrorBuilder().WithRefToEbmsMessageId(refToMessageId).WithAS4Exception(exception).Build();

                return AS4Message.Create(error, GetSendingPMode());
            }

            private HttpRequestMessage CreateSendMessage(AS4Message message)
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, _receiveAgentUrl);

                byte[] serializedMessage;

                using (var stream = new MemoryStream())
                {
                    ISerializer serializer = SerializerProvider.Default.Get(message.ContentType);
                    serializer.Serialize(message, stream, CancellationToken.None);

                    serializedMessage = stream.ToArray();
                }

                requestMessage.Content = new ByteArrayContent(serializedMessage);
                requestMessage.Content.Headers.Add("Content-Type", message.ContentType);

                return requestMessage;
            }

            private void AssertIfInMessageExistsForSignalMessage(string expectedId)
            {
                InMessage inMessage = _databaseSpy.GetInMessageFor(m => m.EbmsRefToMessageId == expectedId);
                Assert.NotNull(inMessage);
                Assert.Equal(InStatus.Received, inMessage.Status);
                Assert.Equal(Operation.ToBeNotified, inMessage.Operation);
            }

            private void AssertIfStatusOfOutMessageIs(string expectedId, OutStatus expectedStatus)
            {
                OutMessage outMessage = _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == expectedId);

                Assert.NotNull(outMessage);
                Assert.Equal(expectedStatus, outMessage.Status);
            }
        }

        // TODO:
        // - Create a test that verifies if the Status for a received receipt/error is set to
        // --> ToBeNotified when the receipt is valid
        // --> Exception when the receipt is invalid (also, an InException should be created)

        // - Create a test that verifies if the Status for a received UserMessage is set to
        // - Exception when the UserMessage is not valid (an InException should be present).
        protected override void Disposing(bool isDisposing)
        {
            _as4Msh.Dispose();
            HttpClient.Dispose();
        }
    }
}