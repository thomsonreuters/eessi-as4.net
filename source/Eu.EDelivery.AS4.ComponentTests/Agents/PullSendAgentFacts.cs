using System;
using System.Xml;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.ComponentTests.Extensions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;
using static Eu.EDelivery.AS4.ComponentTests.Properties.Resources;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class PullSendAgentFacts : ComponentTestTemplate
    {
        private const string SubmitUrl = "http://localhost:7070/msh/";
        private const string PullSendUrl = "http://localhost:8081/msh/";

        private AS4Component _as4Msh;

        public PullSendAgentFacts()
        {
            OverrideSettings("pullsendagent_settings.xml");
            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);
        }

        [Fact]
        public async Task PullSendAgentReturnsUserMessage_ForPullRequestWithEmptyMpc()
        {
            // Arrange
            SubmitMessageToSubmitAgent(pullsendagent_submit).Wait();

            // Act
            HttpResponseMessage userMessageResponse = await StubSender.SendRequest(PullSendUrl, Encoding.UTF8.GetBytes(pullrequest_without_mpc), "application/soap+xml");

            // Assert
            AS4Message as4Message = await userMessageResponse.DeserializeToAS4Message();
            Assert.True(as4Message.IsUserMessage, "AS4 Message isn't a User Message");
            Assert.Equal(Constants.Namespaces.EbmsDefaultMpc, as4Message.PrimaryUserMessage.Mpc);
        }

        [Fact]
        public void TestPullRequestWithoutMpc()
        {
            // Arrange
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(pullrequest_without_mpc);

            var pullRequestNode = doc.SelectSingleNode("//*[local-name()='PullRequest']");

            Assert.True(pullRequestNode != null, "Message does not contain a PullRequest");
            Assert.True(pullRequestNode.Attributes["mpc"] == null, "The PullRequest message has an MPC defined");
        }

        [Fact]
        public async Task TestPullRequestWithSpecifiedMpc()
        {
            string mpc = "http://as4.net.eu/mpc/2";

            var submitMessage = new SubmitMessage()
            {
                MessageInfo = new MessageInfo(null, mpc)
            };

            submitMessage.Collaboration.AgreementRef.PModeId = "pullsendagent-pmode";

            // Arrange
            SubmitMessageToSubmitAgent(AS4XmlSerializer.ToString(submitMessage)).Wait();

            // Act
            HttpResponseMessage userMessageResponse = await StubSender.SendAS4Message(PullSendUrl, CreatePullRequestWithMpc(mpc));

            // Assert
            AS4Message as4Message = await userMessageResponse.DeserializeToAS4Message();
            Assert.True(as4Message.IsUserMessage, "AS4 Message isn't a User Message");
            Assert.Equal(mpc, as4Message.PrimaryUserMessage.Mpc);
        }

        [Fact]
        public async Task RespondsWithBadRequest_WhenInvalidMessageReceived()
        {
            // Act
            HttpResponseMessage response = await StubSender.SendRequest(PullSendUrl, Encoding.UTF8.GetBytes(pullsendagent_submit), "application/soap+xml");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private static AS4Message CreatePullRequestWithMpc(string mpc)
        {
            PullRequest pr = new PullRequest(mpc);
            var pullRequestMessage = AS4Message.Create(pr);

            return pullRequestMessage;
        }

        private static async Task SubmitMessageToSubmitAgent(string submitMessage)
        {
            await StubSender.SendRequest(SubmitUrl, Encoding.UTF8.GetBytes(submitMessage), "application/soap+xml");
            // Wait a bit so that we're sure that the processing agent has picked up the message.
            await Task.Delay(3000);
        }

        protected override void Disposing(bool isDisposing)
        {
            _as4Msh?.Dispose();
            _as4Msh = null;
        }
    }
}
