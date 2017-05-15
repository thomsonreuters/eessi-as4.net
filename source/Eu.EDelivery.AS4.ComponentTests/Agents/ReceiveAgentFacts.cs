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
using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class ReceiveAgentFacts : ComponentTestTemplate
    {
        private readonly AS4Component _as4Msh;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _receiveAgentUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveAgentFacts"/> class.
        /// </summary>
        public ReceiveAgentFacts()
        {
            OverrideSettings("receiveagent_http_settings.xml");

            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);

            var receivingAgent = _as4Msh.GetConfiguration().GetSettingsAgents().FirstOrDefault(a => a.Name.Equals("Receive Agent"));

            if (receivingAgent == null)
            {
                throw new ConfigurationErrorsException("The Agent with name Receive Agent could not be found");
            }

            _receiveAgentUrl = receivingAgent.Receiver?.Setting?.FirstOrDefault(s => s.Key == "Url")?.Value;

            if (String.IsNullOrWhiteSpace(_receiveAgentUrl))
            {
                throw new ConfigurationErrorsException("The URL where the receive agent is listening on, could not be retrieved.");
            }
        }

        public class GivenValidReceivedAS4MessageFacts : ReceiveAgentFacts
        {
            [Fact]
            public async Task ThenInMessageOperationIsToBeDelivered()
            {
                var sendMessage = CreateSendAS4Message();

                var response = await _httpClient.SendAsync(sendMessage);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var receivedAS4Message =
                    await SerializerProvider.Default.Get(response.Content.Headers.ContentType.MediaType)
                                            .DeserializeAsync(await response.Content.ReadAsStreamAsync(),
                                                              response.Content.Headers.ContentType.MediaType, CancellationToken.None);
                Assert.True(receivedAS4Message.IsSignalMessage);
                Assert.True(receivedAS4Message.PrimarySignalMessage is Receipt);

                // Check if the status of the received UserMessage is set to 'ToBeDelivered'
                var spy = new DatabaseSpy(_as4Msh.GetConfiguration());
                var receivedUserMessage = spy.GetInMessageFor(i => i.EbmsMessageId.Equals(receivedAS4Message.PrimarySignalMessage.RefToMessageId));
                Assert.NotNull(receivedUserMessage);
                Assert.Equal(Operation.ToBeDelivered, receivedUserMessage.Operation);
            }

            private HttpRequestMessage CreateSendAS4Message()
            {
                var message = new HttpRequestMessage(HttpMethod.Post, _receiveAgentUrl);

                message.Content = new ByteArrayContent(Properties.Resources.receiveagent_message);
                message.Content.Headers.Add("Content-Type", "multipart/related; boundary=\"=-C3oBZDXCy4W2LpjPUhC4rw==\"; type=\"application/soap+xml\"; charset=\"utf-8\"");

                return message;
            }
        }

        protected override void Disposing(bool isDisposing)
        {
            _as4Msh?.Dispose();
            _httpClient?.Dispose();
        }
    }
}
