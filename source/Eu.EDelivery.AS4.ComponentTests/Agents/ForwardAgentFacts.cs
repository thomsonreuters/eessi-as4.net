using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;
using AgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;
using Service = Eu.EDelivery.AS4.Model.Core.Service;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class ForwardAgentFacts : ComponentTestTemplate
    {
        private readonly AS4Component _as4Msh;
        private readonly DatabaseSpy _databaseSpy;
        private readonly string _receiveAgentUrl;
        private readonly Settings _forwardSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardAgentFacts"/> class.
        /// </summary>
        public ForwardAgentFacts()
        {
            _forwardSettings = OverrideSettings("forwardagent_settings.xml");
            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);
            _databaseSpy = new DatabaseSpy(_as4Msh.GetConfiguration());


            _receiveAgentUrl = _forwardSettings
                .Agents.ReceiveAgents.First().Receiver.Setting
                .FirstOrDefault(s => s.Key == "Url")
                ?.Value;
        }

        protected override void Disposing(bool isDisposing)
        {
            _as4Msh.Dispose();
        }

        [Fact]
        public async Task OutMessageIsCreatedForToBeForwardedMessage()
        {
            // Arrange
            const string messageId = "message-id";
            var as4Message = AS4Message.Create(CreateForwardPushUserMessage(messageId));
            
            // Act: Send an AS4Message to the AS4 MSH which has a receive-agent configured.
            await SendAS4MessageTo(as4Message, _receiveAgentUrl);

            // Assert: if an OutMessage is created with the correct status and operation.
            InMessage inMessage = await PollUntilPresent(
                () => _databaseSpy.GetInMessageFor(
                    m => m.EbmsMessageId == messageId
                         && m.Operation == Operation.Forwarded),
                TimeSpan.FromSeconds(15));
            Assert.NotNull(AS4XmlSerializer.FromString<ReceivingProcessingMode>(inMessage.PMode));

            OutMessage outMessage = _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == messageId);
            Assert.NotNull(outMessage);
            Assert.True(outMessage.Intermediary);
            Assert.Equal(Operation.ToBeProcessed, outMessage.Operation);
            Assert.NotNull(AS4XmlSerializer.FromString<SendingProcessingMode>(outMessage.PMode));
        }

        [Fact]
        public async Task OutMessageIsCreatedForPrimaryMessageUnitOfToBeForwardedAS4Message()
        {
            // Arrange
            const string primaryMessageId = "primary-message-id";
            const string secondMessageId = "secondary-message-id";

            var primaryUserMessage = CreateForwardPushUserMessage(primaryMessageId);
            var secondaryUserMessage = new UserMessage(secondMessageId);

            var as4Message = AS4Message.Create(primaryUserMessage);
            as4Message.AddMessageUnit(secondaryUserMessage);

            // Act: Send an AS4Message to the AS4 MSH which has a receive-agent configured
            await SendAS4MessageTo(as4Message, _receiveAgentUrl);

            // Assert: if an OutMessage is created with the correct status and operation.
            InMessage primaryInMessage = await PollUntilPresent(
                () => _databaseSpy.GetInMessageFor(
                    m => m.EbmsMessageId == primaryMessageId
                         && m.Operation == Operation.Forwarded),
                TimeSpan.FromSeconds(15));
            Assert.NotNull(AS4XmlSerializer.FromString<ReceivingProcessingMode>(primaryInMessage.PMode));

            var secondaryInMessage = _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == secondMessageId);
            Assert.NotNull(secondaryInMessage);
            Assert.Equal(Operation.Forwarded, primaryInMessage.Operation);
            Assert.NotNull(AS4XmlSerializer.FromString<ReceivingProcessingMode>(primaryInMessage.PMode));

            var primaryOutMessage = _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == primaryMessageId);
            Assert.NotNull(primaryOutMessage);
            Assert.Equal(Operation.ToBeProcessed, primaryOutMessage.Operation);
            Assert.NotNull(AS4XmlSerializer.FromString<SendingProcessingMode>(primaryOutMessage.PMode));

            var secondaryOutMessage = _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == secondMessageId);
            Assert.Null(secondaryOutMessage);
        }

        [Fact]
        public async Task ForwardingWithPullOnPush()
        {
            // Arrange
            const string messageId = "message-id";
            AS4Message as4Message = AS4Message.Create(CreateForwardPullUserMessage(messageId));

            // Act: Send an AS4Message to the AS4 MSH which has a receive-agent configured.
            await SendAS4MessageTo(as4Message, _receiveAgentUrl);

            // Assert: if an OutMessage is created with the correct status and operation.
            InMessage inMessage = await PollUntilPresent(
                () => _databaseSpy.GetInMessageFor(
                    m => m.EbmsMessageId == messageId
                         && m.Operation == Operation.Forwarded), 
                TimeSpan.FromSeconds(15));

            var receivingPMode = AS4XmlSerializer.FromString<ReceivingProcessingMode>(inMessage.PMode);
            Assert.NotNull(receivingPMode);

            OutMessage outMessage = await PollUntilPresent(
                () => _databaseSpy.GetOutMessageFor(
                    m => m.EbmsMessageId == messageId
                         && m.Operation == Operation.ToBeProcessed),
                TimeSpan.FromSeconds(5));

            var sendingPMode = AS4XmlSerializer.FromString<SendingProcessingMode>(outMessage.PMode);
            Assert.NotNull(sendingPMode);
            Assert.Equal(MessageExchangePattern.Pull, outMessage.MEP);
            Assert.Equal(sendingPMode.MessagePackaging.Mpc, outMessage.Mpc);
        }

        private static UserMessage CreateForwardPullUserMessage(string messageId)
        {
            return new UserMessage(
                messageId,
                new CollaborationInfo(
                    agreement: new AgreementReference(
                        value: "http://agreements.europa.org/agreement",
                        pmodeId: "Forward_Pull"),
                    service: new Service(
                        value: "Forward_Pull_Service",
                        type: "eu:europa:services"),
                    action: "Forward_Pull_Action",
                    conversationId: "eu:europe:conversation"));
        }

        private static async Task SendAS4MessageTo(AS4Message msg, string url)
        {
            HttpResponseMessage response = await StubSender.SendAS4Message(url, msg);
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }

        [Fact]
        public async Task ForwardingWithPushOnPull()
        {
            // Arrange
            const string messageId = "user-message-id";
            var tobeForwarded = AS4Message.Create(CreateForwardPushUserMessage(messageId));

            // Act: Start a Stub HTTP Server that listens on the PullRequest endpoint
            // and replies with an AS4 UserMessage
            RespondToPullRequestWith(tobeForwarded);

            // Assert: if an OutMessage is created with the correct status and operation
            await PollUntilPresent(
                () => _databaseSpy.GetInMessageFor(m => 
                    m.EbmsMessageId == messageId 
                    && m.Operation == Operation.Forwarded), 
                TimeSpan.FromSeconds(15));

            OutMessage outMessage = await PollUntilPresent(
                () => _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == messageId),
                TimeSpan.FromSeconds(5));

            var sendingPMode = AS4XmlSerializer.FromString<SendingProcessingMode>(outMessage.PMode);
            Assert.NotNull(sendingPMode);
            Assert.Equal(Operation.ToBeProcessed, outMessage.Operation);
            Assert.Equal(MessageExchangePattern.Push, outMessage.MEP);
            Assert.Equal(sendingPMode.MessagePackaging.Mpc, outMessage.Mpc);
        }

        private void RespondToPullRequestWith(AS4Message tobeForwarded)
        {
            string pullingUrl = RetrievePullingUrlFromConfig();
            var responseHandler = new AS4MessageResponseHandler(tobeForwarded);

            var waiter = new ManualResetEvent(false);
            StubHttpServer.StartServer(pullingUrl, responseHandler.WriteResponse, waiter);
            Assert.True(waiter.WaitOne(TimeSpan.FromSeconds(15)));
        }

        private string RetrievePullingUrlFromConfig()
        {
            var pullReceiveAgent = _forwardSettings.Agents.PullReceiveAgents.FirstOrDefault();

            if (pullReceiveAgent == null)
            {
                throw new ConfigurationErrorsException("There is no PullReceive Agent configured.");
            }

            string pmodeId = pullReceiveAgent.Receiver.Setting.First().Key;

            var pmode = AS4XmlSerializer.FromString<SendingProcessingMode>(
                File.ReadAllText($@".\config\send-pmodes\{pmodeId.ToLower()}.xml"));

            if (pmode == null)
            {
                throw new ConfigurationErrorsException($"No Sending PMode found with Id {pmodeId}");
            }

            return pmode.PushConfiguration.Protocol.Url;
        }

        private static  UserMessage CreateForwardPushUserMessage(string messageId)
        {
            return new UserMessage(
                messageId,
                new CollaborationInfo(
                    agreement: new AgreementReference(
                        value: "http://agreements.europa.org/agreement",
                        pmodeId: "Forward_Push"),
                    service: new Service(
                        value: "Forward_Push_Service",
                        type: "eu:europa:services"),
                    action: "Forward_Push_Action",
                    conversationId: "eu:europe:conversation"));
        }
    }
}

