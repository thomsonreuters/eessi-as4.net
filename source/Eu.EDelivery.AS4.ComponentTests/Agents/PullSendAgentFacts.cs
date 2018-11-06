using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.ComponentTests.Extensions;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;
using static Eu.EDelivery.AS4.ComponentTests.Properties.Resources;
using AgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class PullSendAgentFacts : ComponentTestTemplate
    {
        private const string SubmitUrl = "http://localhost:7070/msh/";
        private const string PullSendUrl = "http://localhost:8081/msh/";
        private const string AllowedPullRequestMpc = "componenttest-mpc";

        private readonly AS4Component _as4Msh;
        private readonly DatabaseSpy _databaseSpy;

        public PullSendAgentFacts()
        {
            OverrideSettings("pullsendagent_settings.xml");
            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);
            _databaseSpy = new DatabaseSpy(_as4Msh.GetConfiguration());
        }

        [Fact]
        public async Task PullSendAgentReturnsUserMessage_ForPullRequestWithEmptyMpc()
        {
            // Arrange
            var userMessage = 
                AS4Message.Create(
                    new UserMessage(
                    $"user-{Guid.NewGuid()}",
                    new CollaborationInfo(
                        new AgreementReference(
                            value: "http://eu.europe.agreements.org",
                            pmodeId: "pullsendagent-pmode"))));

            var outMessage = new OutMessage(userMessage.GetPrimaryMessageId())
            {
                ContentType = userMessage.ContentType,
                MEP = MessageExchangePattern.Pull,
                Operation = Operation.ToBeSent,
                MessageLocation = 
                    Registry.Instance
                            .MessageBodyStore
                            .SaveAS4Message(_as4Msh.GetConfiguration().OutExceptionStoreLocation, userMessage)
            };

            outMessage.AssignAS4Properties(userMessage.PrimaryMessageUnit);
            _databaseSpy.InsertOutMessage(outMessage);

            // Act
            HttpResponseMessage userMessageResponse = 
                await StubSender.SendRequest(PullSendUrl, Encoding.UTF8.GetBytes(pullrequest_without_mpc), "application/soap+xml");

            // Assert
            AS4Message as4Message = await userMessageResponse.DeserializeToAS4Message();
            Assert.True(as4Message.IsUserMessage, "AS4 Message isn't a User Message");
            Assert.Equal(Constants.Namespaces.EbmsDefaultMpc, as4Message.FirstUserMessage.Mpc);
        }

        [Fact]
        public void TestPullRequestWithoutMpc()
        {
            // Arrange
            var doc = new XmlDocument();
            doc.LoadXml(pullrequest_without_mpc);

            XmlNode pullRequestNode = doc.SelectSingleNode("//*[local-name()='PullRequest']");

            Assert.True(pullRequestNode != null, "Message does not contain a PullRequest");
            Assert.True(pullRequestNode.Attributes != null, "The PullRequest message does not has attributes");
            Assert.True(pullRequestNode.Attributes["mpc"] == null, "The PullRequest message does not has an MPC defined");
        }

        [Fact]
        public async Task TestPullRequestWithSpecifiedMpc()
        {
            // Arrange
            const string mpc = "http://as4.net.eu/mpc/2";
            await StoreToBeSentUserMessage(mpc);

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
            HttpResponseMessage response = 
                await StubSender.SendRequest(PullSendUrl, Encoding.UTF8.GetBytes(pullsendagent_submit), "application/soap+xml");

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

            AS4Message pullRequest = CreateAllowedPullRequest();
            AS4Message signedPullRequest = SignAS4MessageWithPullRequestCert(pullRequest);

            // Act
            HttpResponseMessage response = await StubSender.SendAS4Message(PullSendUrl, signedPullRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PiggyBacked_PullRequest_With_Bundled_Error_Respond_No_UserMessage_Available()
        {
            HttpResponseMessage response =
                await PiggyBacked_PullRequest_With_Bundled_Signal(
                    CreateErrorRefTo,
                    OutStatus.Nack);

            await AssertResponseIsPullRequestWarning(response);
        }

        [Fact]
        public async Task PiggyBacked_PullRequest_With_Bundled_Error_Respond_With_UserMessage()
        {
            // Arrange
            await StoreToBeSentUserMessage(AllowedPullRequestMpc);

            // Act
            HttpResponseMessage response =
                await PiggyBacked_PullRequest_With_Bundled_Signal(CreateErrorRefTo, OutStatus.Nack);

            // Assert
            await AssertRespondseIsAvailableUserMessage(response);
        }

        private static Error CreateErrorRefTo(string userMessageId)
        {
            return new Error(
                $"error-{Guid.NewGuid()}",
                userMessageId,
                new ErrorLine(
                    ErrorCode.Ebms0004,
                    Severity.FAILURE,
                    ErrorAlias.Other));
        }

        [Fact]
        public async Task PiggyBacked_PullRequest_With_Bundled_Receipt_Respond_No_UserMessage_Available()
        {
            HttpResponseMessage response =
                await PiggyBacked_PullRequest_With_Bundled_Signal(CreateReceiptRefTo, OutStatus.Ack);

            await AssertResponseIsPullRequestWarning(response);
        }

        [Fact]
        public async Task PiggyBacked_PullRequest_With_Bundled_Receipt_Respond_With_UserMessage()
        {
            // Arrange
            await StoreToBeSentUserMessage(AllowedPullRequestMpc);

            // Act
            HttpResponseMessage response =
                await PiggyBacked_PullRequest_With_Bundled_Signal(CreateReceiptRefTo, OutStatus.Ack);

            // Assert
            await AssertRespondseIsAvailableUserMessage(response);
        }

        private static async Task StoreToBeSentUserMessage(string mpc)
        {
            var submit = new SubmitMessage
            {
                MessageInfo = new MessageInfo(messageId: null, mpc: mpc),
                Collaboration =
                {
                    AgreementRef =
                    {
                        PModeId = "pullsendagent-pmode"
                    }
                }
            };

            await SubmitMessageToSubmitAgent(AS4XmlSerializer.ToString(submit));
        }

        private static Receipt CreateReceiptRefTo(string userMessageId)
        {
            return new Receipt(
                messageId: $"receipt-{Guid.NewGuid()}",
                refToMessageId: userMessageId,
                timestamp: DateTimeOffset.Now);
        }

        private async Task<HttpResponseMessage> PiggyBacked_PullRequest_With_Bundled_Signal(
            Func<string, SignalMessage> createSignal,
            OutStatus expStatus)
        {
            // Arrange
            OverridePullAuthorizationMap(
                @".\config\componenttest-settings\security\pull_authorizationmap_allowed_facts.xml");

            string userMessageId = $"user-{Guid.NewGuid()}";
            StoreToBeAckOutMessage(userMessageId, CreateSendingPMode());

            AS4Message pullRequest = CreateAllowedPullRequest();
            SignalMessage extraSignalMessage = createSignal(userMessageId);
            pullRequest.AddMessageUnit(extraSignalMessage);

            AS4Message signedBundled = SignAS4MessageWithPullRequestCert(pullRequest);

            // Act
            HttpResponseMessage response = await StubSender.SendAS4Message(PullSendUrl, signedBundled);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            OutMessage storedUserMesage =
                _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == userMessageId);
            Assert.Equal(expStatus, storedUserMesage.Status.ToEnum<OutStatus>());

            InMessage storedSignalMessage =
                _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == extraSignalMessage.MessageId);
            Assert.Equal(InStatus.Received, storedSignalMessage.Status.ToEnum<InStatus>());

            return response;
        }

        private static async Task AssertRespondseIsAvailableUserMessage(HttpResponseMessage response)
        {
            AS4Message actual = await response.DeserializeToAS4Message();
            var userMessage = actual.PrimaryMessageUnit as UserMessage;
            Assert.NotNull(userMessage);
            Assert.Equal(AllowedPullRequestMpc, userMessage.Mpc);
        }

        private static async Task AssertResponseIsPullRequestWarning(HttpResponseMessage response)
        {
            AS4Message as4Message = await response.DeserializeToAS4Message();
            var error = as4Message.PrimaryMessageUnit as Error;
            Assert.NotNull(error);
            Assert.True(error.IsWarningForEmptyPullRequest, "Responded Error is not a PullRequest warning");
        }

        private void StoreToBeAckOutMessage(string messageId, SendingProcessingMode sendingPMode)
        {
            var outMessage = new OutMessage(messageId);

            outMessage.SetStatus(OutStatus.Sent);
            outMessage.SetPModeInformation(sendingPMode);

            _databaseSpy.InsertOutMessage(outMessage);
        }

        private static SendingProcessingMode CreateSendingPMode()
        {
            return new SendingProcessingMode
            {
                Id = "pullsend_agent_facts_pmode",
                ReceiptHandling = { NotifyMessageProducer = true },
                ErrorHandling = { NotifyMessageProducer = true }
            };
        }

        private static AS4Message CreateAllowedPullRequest()
        {
            return CreatePullRequestWithMpc("componenttest-mpc");
        }

        private static AS4Message CreatePullRequestWithMpc(string mpc)
        {
            return AS4Message.Create(new PullRequest(mpc));
        }

        private static AS4Message SignAS4MessageWithPullRequestCert(AS4Message message)
        {
            var certificate = 
                new X509Certificate2(
                    @".\samples\certificates\AccessPointA.pfx",
                    "Pz4cZK4SULUwmraZa",
                    X509KeyStorageFlags.Exportable);

            var config = new CalculateSignatureConfig(certificate,
                X509ReferenceType.BSTReference,
                Constants.SignAlgorithms.Sha256,
                Constants.HashFunctions.Sha256);

            message.SecurityHeader.Sign(
                SignStrategy.ForAS4Message(message, config));

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
            _as4Msh.Dispose();
        }
    }
}
