using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.ComponentTests.Extensions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Serialization;
using Xunit;
using static Eu.EDelivery.AS4.ComponentTests.Properties.Resources;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class PullSendAgentFacts : ComponentTestTemplate
    {
        private const string SubmitUrl = "http://localhost:7070/msh/";
        private const string PullSendUrl = "http://localhost:8081/msh/";

        private static readonly HttpClient HttpClient = new HttpClient();
        private AS4Component _as4Msh;

        [Fact]
        public async Task PullSendAgentReturnsUserMessage_ForPullRequestWithEmptyMpc()
        {
            // Before
            OverrideSettings("pullsendagent_settings.xml");
            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);

            // Arrange
            SubmitMessageToSubmitAgent(pullsendagent_submit).Wait();

            // Act
            HttpResponseMessage userMessageResponse =
                await HttpClient.SendAsync(CreateHttpRequestFrom(PullSendUrl, pullrequest_without_mpc));

            // Assert
            AS4Message as4Message = await userMessageResponse.DeserializeToAS4Message();
            Assert.True(as4Message.IsUserMessage, "AS4 Message isn't a User Message");
            Assert.Equal(Constants.Namespaces.EbmsDefaultMpc, as4Message.PrimaryUserMessage.Mpc);
        }

        [Fact]
        public async Task TestPullRequestWithoutMpc()
        {
            // Act
            AS4Message as4Message = await DeserializeSoapXml(pullrequest_without_mpc);

            // Assert
            Assert.True(as4Message.IsPullRequest, "AS4 Message isn't a Pull Request");

            var pullRequest = (PullRequest) as4Message.PrimarySignalMessage;
            Assert.True(string.IsNullOrEmpty(pullRequest.Mpc), "Pull Request hasn't got empty MPC");
        }

        [Fact]
        public async Task TestPullRequestWithSpecifiedMpc()
        {
            // Before
            OverrideSettings("pullsendagent_settings.xml");
            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);

            string mpc = "http://as4.net.eu/mpc/2";

            var submitMessage = new SubmitMessage()
            {                
                MessageInfo = new MessageInfo(null, mpc)
            };

            submitMessage.Collaboration.AgreementRef.PModeId = "pullsendagent-pmode";

            // Arrange
            SubmitMessageToSubmitAgent(AS4XmlSerializer.ToString(submitMessage)).Wait();

            // Act
            
            HttpResponseMessage userMessageResponse =
                await HttpClient.SendAsync(CreateHttpRequestFrom(PullSendUrl, CreatePullRequestWithMpc(mpc)));

            // Assert
            AS4Message as4Message = await userMessageResponse.DeserializeToAS4Message();
            Assert.True(as4Message.IsUserMessage, "AS4 Message isn't a User Message");
            Assert.Equal(mpc, as4Message.PrimaryUserMessage.Mpc);
        }

        private static string CreatePullRequestWithMpc(string mpc)
        {
            PullRequest pr = new PullRequest(mpc);
            var pullRequestMessage = AS4Message.Create(pr);

            using (var stream = new MemoryStream())
            {
                var serializer = SerializerProvider.Default.Get(pullRequestMessage.ContentType);
                serializer.Serialize(pullRequestMessage, stream, CancellationToken.None);

                stream.Position = 0;

                return System.Text.Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        private static async Task<AS4Message> DeserializeSoapXml(string soapXml)
        {
            using (var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(soapXml)))
            {
                var serializer = new SoapEnvelopeSerializer();
                return await serializer.DeserializeAsync(contentStream, "application/soap+xml", CancellationToken.None);
            }
        }

        private static async Task SubmitMessageToSubmitAgent(string submitMessage)
        {
            await HttpClient.SendAsync(CreateHttpRequestFrom(SubmitUrl, submitMessage));
            // Wait a bit so that we're sure that the processing agent has picked up the message.
            await Task.Delay(3000);
        }

        private static HttpRequestMessage CreateHttpRequestFrom(string url, string message)
        {
            return new HttpRequestMessage(HttpMethod.Post, url) {Content = new StringContent(message, Encoding.UTF8, "application/soap+xml")};
        }

        protected override void Disposing(bool isDisposing)
        {
            _as4Msh?.Dispose();
        }
    }
}
