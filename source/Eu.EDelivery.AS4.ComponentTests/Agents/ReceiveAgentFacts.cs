using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
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
            
            Assert.False(string.IsNullOrWhiteSpace(_receiveAgentUrl), "The URL where the receive agent is listening on, could not be retrieved.");
        }

        public class GivenValidReceivedAS4MessageFacts : ReceiveAgentFacts
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
                           inputStream: await response.Content.ReadAsStreamAsync(),
                           contentType: response.Content.Headers.ContentType.MediaType,
                           cancellationToken: CancellationToken.None);
            }

            private InMessage GetInsertedUserMessageFor(AS4Message receivedAS4Message)
            {
                return
                    _databaseSpy.GetInMessageFor(
                        i => i.EbmsMessageId.Equals(receivedAS4Message.PrimarySignalMessage.RefToMessageId));
            }
        }

        // TODO:
        // - Create a test that verifies if the Status for a received receipt/error is set to
        // - ToBeNotified when the receipt is valid
        // - Exception when the receipt is invalid (also, an InException should be created)

        // - Create a test that verifies if the Status for a received UserMessage is set to
        // - Exception when the UserMessage is not valid (an InException should be present).
        protected override void Disposing(bool isDisposing)
        {
            _as4Msh?.Dispose();
            HttpClient?.Dispose();
        }
    }
}