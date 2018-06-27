using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;
using AgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;

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
            const string messageId = "message-id";

            // Send an AS4Message to the AS4 MSH which has a receive-agent configured.
            var as4Message = CreateAS4Message("Forward_Push", new UserMessage(messageId));

            var response = await StubSender.SendAS4Message(_receiveAgentUrl, as4Message);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            await Task.Delay(TimeSpan.FromSeconds(3));

            // Assert if an OutMessage is created with the correct status and operation.
            var inMessage = _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == messageId);
            var outMessage = _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == messageId);

            Assert.NotNull(inMessage);
            Assert.Equal(Operation.Forwarded, inMessage.Operation.ToEnum<Operation>());
            Assert.NotNull(AS4XmlSerializer.FromString<ReceivingProcessingMode>(inMessage.PMode));

            Assert.NotNull(outMessage);
            Assert.True(outMessage.Intermediary);
            Assert.Equal(Operation.ToBeProcessed, outMessage.Operation.ToEnum<Operation>());
            Assert.NotNull(AS4XmlSerializer.FromString<SendingProcessingMode>(outMessage.PMode));
        }

        [Fact]
        public async Task OutMessageIsCreatedForPrimaryMessageUnitOfToBeForwardedAS4Message()
        {
            const string messageId = "primary-message-id";
            const string secondMessageId = "secondary-message-id";

            // Send an AS4Message to the AS4 MSH which has a receive-agent configured.
            var as4Message = CreateAS4Message("Forward_Push", new UserMessage(messageId), new UserMessage(secondMessageId));

            var response = await StubSender.SendAS4Message(_receiveAgentUrl, as4Message);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            await Task.Delay(TimeSpan.FromSeconds(3));

            // Assert if an OutMessage is created with the correct status and operation.
            var primaryInMessage = _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == messageId);
            var secondaryInMessage = _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == secondMessageId);
            var primaryOutMessage = _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == messageId);
            var secondaryOutMessage = _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == secondMessageId);

            Assert.NotNull(primaryInMessage);
            Assert.Equal(Operation.Forwarded, primaryInMessage.Operation.ToEnum<Operation>());
            Assert.NotNull(AS4XmlSerializer.FromString<ReceivingProcessingMode>(primaryInMessage.PMode));

            Assert.NotNull(secondaryInMessage);
            Assert.Equal(Operation.Forwarded, primaryInMessage.Operation.ToEnum<Operation>());
            Assert.NotNull(AS4XmlSerializer.FromString<ReceivingProcessingMode>(primaryInMessage.PMode));

            Assert.NotNull(primaryOutMessage);
            Assert.Equal(Operation.ToBeProcessed, primaryOutMessage.Operation.ToEnum<Operation>());
            Assert.NotNull(AS4XmlSerializer.FromString<SendingProcessingMode>(primaryOutMessage.PMode));

            Assert.Null(secondaryOutMessage);
        }

        [Fact]
        public async Task ForwardingWithPullOnPush()
        {
            const string messageId = "message-id";

            // Send an AS4Message to the AS4 MSH which has a receive-agent configured.
            var as4Message = CreateAS4Message("Forward_Pull", new UserMessage(messageId));

            var response = await StubSender.SendAS4Message(_receiveAgentUrl, as4Message);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            await Task.Delay(TimeSpan.FromSeconds(3));

            // Assert if an OutMessage is created with the correct status and operation.
            var inMessage = _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == messageId);
            var outMessage = _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == messageId);

            Assert.NotNull(inMessage);
            Assert.Equal(Operation.Forwarded, inMessage.Operation.ToEnum<Operation>());

            var receivingPMode = AS4XmlSerializer.FromString<ReceivingProcessingMode>(inMessage.PMode);

            Assert.NotNull(receivingPMode);

            Assert.NotNull(outMessage);

            var sendingPMode = AS4XmlSerializer.FromString<SendingProcessingMode>(outMessage.PMode);

            Assert.NotNull(sendingPMode);
            Assert.Equal(Operation.ToBeProcessed, outMessage.Operation.ToEnum<Operation>());
            Assert.Equal(MessageExchangePattern.Pull, outMessage.MEP.ToEnum<MessageExchangePattern>());
            Assert.Equal(sendingPMode.MessagePackaging.Mpc, outMessage.Mpc);
        }

        [Fact]
        public async Task ForwardingWithPushOnPull()
        {
            string pullingUrl = RetrievePullingUrlFromConfig();

            var waiter = new ManualResetEvent(false);

            const string messageId = "user-message-id";

            var responseHandler = new AS4MessageResponseHandler(CreateAS4Message("Forward_Push", new UserMessage(messageId)));

            // Start a Stub HTTP Server that listens on the PullRequest endpoint and
            // replies with an AS4 UserMessage.
            StubHttpServer.StartServer(pullingUrl, responseHandler.WriteResponse, waiter);

            Assert.True(waiter.WaitOne(TimeSpan.FromSeconds(15)));

            // Wait a little bit so that the Message can be processed.
            await Task.Delay(TimeSpan.FromSeconds(5));

            // Assert if an OutMessage is created with the correct status and operation.
            var inMessage = _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == messageId);
            var outMessage = _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == messageId);

            Assert.NotNull(inMessage);
            Assert.Equal(Operation.Forwarded, inMessage.Operation.ToEnum<Operation>());

            var receivingPMode = AS4XmlSerializer.FromString<ReceivingProcessingMode>(inMessage.PMode);

            Assert.NotNull(receivingPMode);

            Assert.NotNull(outMessage);

            var sendingPMode = AS4XmlSerializer.FromString<SendingProcessingMode>(outMessage.PMode);

            Assert.NotNull(sendingPMode);
            Assert.Equal(Operation.ToBeProcessed, outMessage.Operation.ToEnum<Operation>());
            Assert.Equal(MessageExchangePattern.Push, outMessage.MEP.ToEnum<MessageExchangePattern>());
            Assert.Equal(sendingPMode.MessagePackaging.Mpc, outMessage.Mpc);

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

        /// <summary>
        /// Creates an AS4 Message that can be send to a AS4.NET receive agent.
        /// </summary>
        /// <param name="pmodeId">The ID of the Receiving PMode that must be used by the Receive-Agent to handle the message.</param>
        /// <param name="messageUnits"></param>
        /// <returns></returns>
        private AS4Message CreateAS4Message(string pmodeId, params MessageUnit[] messageUnits)
        {
            if (messageUnits.Any() == false)
            {
                throw new ArgumentException(@"At least one messageUnit must be specified", nameof(messageUnits));
            }

            var as4Message = AS4Message.Create(messageUnits.First(), new SendingProcessingMode());

            foreach (var mu in messageUnits.Skip(1))
            {
                as4Message.AddMessageUnit(mu);
            }

            // Add a PMode Id to the primary usermessage, just to be sure that it can be picked up with
            // and processed with a specific receiving PMode.
            if (as4Message.FirstUserMessage != null)
            {
                as4Message.FirstUserMessage.CollaborationInfo = 
                    new CollaborationInfo(new AgreementReference(
                        value: String.Empty,
                        type: String.Empty,
                        pModeId: pmodeId));
            }

            return as4Message;
        }
    }
}

