using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.ComponentTests.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Xunit;
using static Eu.EDelivery.AS4.ComponentTests.Properties.Resources;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class PullSendAgentFacts : ComponentTestTemplate
    {
        private const string SubmitUrl = "http://localhost:7070/msh/";
        private const string PullSendUrl = "http://localhost:8081/msh/";

        private static readonly HttpClient HttpClient = new HttpClient();
        private readonly AS4Component _as4Msh;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullSendAgentFacts"/> class.
        /// </summary>
        public PullSendAgentFacts()
        {
            OverrideSettings("pullsendagent_settings.xml");
            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);
        }

        [Fact]
        public async Task PullSendAgentReturnsUserMessage_ForPullRequestWithEmptyMpc()
        {
            // Arrange
            await HttpClient.SendAsync(CreateHttpRequestFrom(SubmitUrl, pullsendagent_submit)); 

            // Act
            HttpResponseMessage userMessageResponse =
                await HttpClient.SendAsync(CreateHttpRequestFrom(PullSendUrl, pullsendagent_pullrequest));

            // Assert
            AS4Message as4Message = await userMessageResponse.DeserializeToAS4Message();
            Assert.True(as4Message.IsUserMessage);
        }

        private static HttpRequestMessage CreateHttpRequestFrom(string url, string message)
        {
            return new HttpRequestMessage(HttpMethod.Post, url) {Content = new StringContent(message, Encoding.UTF8, "application/soap+xml")};
        }

        protected override void Disposing(bool isDisposing)
        {
            _as4Msh.Dispose();
        }
    }
}
