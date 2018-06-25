using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.ComponentTests.Extensions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Strategies;
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
            Assert.Equal(Constants.Namespaces.EbmsDefaultMpc, as4Message.FirstUserMessage.Mpc);
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
            Assert.Equal(mpc, as4Message.FirstUserMessage.Mpc);
        }

        [Fact]
        public async Task RespondsWithBadRequest_WhenInvalidMessageReceived()
        {
            // Act
            HttpResponseMessage response = await StubSender.SendRequest(PullSendUrl, Encoding.UTF8.GetBytes(pullsendagent_submit), "application/soap+xml");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task RespondsWithHttpForbidden_WhenReceivedPullRequestIsNotAllowed()
        {
            OverridePullAuthorizationMap(@".\config\componenttest-settings\security\pull_authorizationmap_notallowed_facts.xml");

            var pullRequest = CreatePullRequestWithMpc("componenttest-mpc");

            // Act
            HttpResponseMessage response = await StubSender.SendAS4Message(PullSendUrl, pullRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task PullSendAgentReturnsHttpOk_IfPullRequestIsAllowed()
        {
            OverridePullAuthorizationMap(@".\config\componenttest-settings\security\pull_authorizationmap_allowed_facts.xml");

            var pullRequest = CreatePullRequestWithMpc("componenttest-mpc");

            var signedPullRequest = SignPullRequest(pullRequest, new X509Certificate2(@".\samples\certificates\AccessPointA.pfx", "Pz4cZK4SULUwmraZa", X509KeyStorageFlags.Exportable));

            // Act
            HttpResponseMessage response = await StubSender.SendAS4Message(PullSendUrl, signedPullRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private static AS4Message CreatePullRequestWithMpc(string mpc)
        {
            PullRequest pr = new PullRequest(mpc);
            var pullRequestMessage = AS4Message.Create(pr);

            return pullRequestMessage;
        }

        private static AS4Message SignPullRequest(AS4Message message, X509Certificate2 certificate)
        {
            
            CalculateSignatureConfig config = new CalculateSignatureConfig(certificate,
                X509ReferenceType.BSTReference,
                Constants.SignAlgorithms.Sha256,
                Constants.HashFunctions.Sha256);

            var signer = SignStrategy.ForAS4Message(message, config);

            message.SecurityHeader.Sign(signer);

            return message;
        }

        private static async Task SubmitMessageToSubmitAgent(string submitMessage)
        {
            await StubSender.SendRequest(SubmitUrl, Encoding.UTF8.GetBytes(submitMessage), "application/soap+xml");
            // Wait a bit so that we're sure that the processing agent has picked up the message.
            await Task.Delay(3000);
        }

        private static void OverridePullAuthorizationMap(string pullAuthorizationMapToUse)
        {
            File.Copy(@".\config\security\pull_authorizationmap.xml", @".\config\security\pull_authorizationmap_original.xml", true);
            File.Copy(pullAuthorizationMapToUse, @".\config\security\pull_authorizationmap.xml", true);
        }

        private static void RestorePullAuthorizationMap()
        {
            const string originalAuthorizationMapBackupFile = @".\config\security\pull_authorizationmap_original.xml";

            if (File.Exists(originalAuthorizationMapBackupFile))
            {
                File.Copy(originalAuthorizationMapBackupFile, @".\config\security\pull_authorizationmap.xml", true);
                File.Delete(originalAuthorizationMapBackupFile);
            }
        }

        protected override void Disposing(bool isDisposing)
        {
            RestorePullAuthorizationMap();
            _as4Msh?.Dispose();
            _as4Msh = null;
        }
    }
}
