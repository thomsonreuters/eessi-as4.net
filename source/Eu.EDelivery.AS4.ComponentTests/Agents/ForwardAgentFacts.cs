using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class ForwardAgentFacts : ComponentTestTemplate
    {
        private readonly AS4Component _as4Msh;
        private readonly DatabaseSpy _databaseSpy;
        private readonly string _receiveAgentUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardAgentFacts"/> class.
        /// </summary>
        public ForwardAgentFacts()
        {
            string RetrieveReceiveAgentUrl(AS4Component as4Component)
            {
                var receivingAgent =
                    as4Component.GetConfiguration().GetAgentsConfiguration().FirstOrDefault(a => a.Type == AgentType.Receive);

                Assert.True(receivingAgent != null, "The Agent with name Receive Agent could not be found");

                return receivingAgent.Settings.Receiver?.Setting?.FirstOrDefault(s => s.Key == "Url")?.Value;
            }

            OverrideSettings("forwardagent_settings.xml");
            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);
            _databaseSpy = new DatabaseSpy(_as4Msh.GetConfiguration());
            _receiveAgentUrl = RetrieveReceiveAgentUrl(_as4Msh);
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
            Assert.Equal(Operation.Forwarded, OperationUtils.Parse(inMessage.Operation));
            Assert.NotNull(AS4XmlSerializer.FromString<ReceivingProcessingMode>(inMessage.PMode));

            Assert.NotNull(outMessage);
            Assert.Equal(Operation.ToBeSent, OperationUtils.Parse(outMessage.Operation));
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
            Assert.Equal(Operation.Forwarded, OperationUtils.Parse(primaryInMessage.Operation));
            Assert.NotNull(AS4XmlSerializer.FromString<ReceivingProcessingMode>(primaryInMessage.PMode));

            Assert.NotNull(secondaryInMessage);
            Assert.Equal(Operation.Forwarded, OperationUtils.Parse(primaryInMessage.Operation));
            Assert.NotNull(AS4XmlSerializer.FromString<ReceivingProcessingMode>(primaryInMessage.PMode));

            Assert.NotNull(primaryOutMessage);
            Assert.Equal(Operation.ToBeSent, OperationUtils.Parse(primaryOutMessage.Operation));
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
            Assert.Equal(Operation.Forwarded, OperationUtils.Parse(inMessage.Operation));

            var receivingPMode = AS4XmlSerializer.FromString<ReceivingProcessingMode>(inMessage.PMode);

            Assert.NotNull(receivingPMode);

            Assert.NotNull(outMessage);

            var sendingPMode = AS4XmlSerializer.FromString<SendingProcessingMode>(outMessage.PMode);

            Assert.NotNull(sendingPMode);
            Assert.Equal(Operation.ToBeSent, OperationUtils.Parse(outMessage.Operation));
            Assert.Equal(MessageExchangePattern.Pull, MessageExchangePatternUtils.Parse(outMessage.MEP));
            Assert.Equal(sendingPMode.MessagePackaging.Mpc, outMessage.Mpc);
        }

        [Fact]
        public async Task ForwardingWithPushOnPull()
        {
            string pullingUrl = RetrievePullingUrlFromConfig(_as4Msh.GetConfiguration());

            var waiter = new ManualResetEvent(false);

            const string messageId = "user-message-id";

            var responseHandler = new AS4MessageResponseHandler(CreateAS4Message("Forward_Push", new UserMessage(messageId)));

            // Start a Stub HTTP Server that listens on the PullRequest endpoint and
            // replies with an AS4 UserMessage.
            StubHttpServer.StartServer(pullingUrl, responseHandler.WriteResponse, waiter);

            Assert.True(waiter.WaitOne(TimeSpan.FromSeconds(15)));

            // Wait a little bit so that the Message can be processed.
            await Task.Delay(TimeSpan.FromSeconds(3));

            // Assert if an OutMessage is created with the correct status and operation.
            var inMessage = _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == messageId);
            var outMessage = _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == messageId);

            Assert.NotNull(inMessage);
            Assert.Equal(Operation.Forwarded, OperationUtils.Parse(inMessage.Operation));

            var receivingPMode = AS4XmlSerializer.FromString<ReceivingProcessingMode>(inMessage.PMode);

            Assert.NotNull(receivingPMode);

            Assert.NotNull(outMessage);

            var sendingPMode = AS4XmlSerializer.FromString<SendingProcessingMode>(outMessage.PMode);

            Assert.NotNull(sendingPMode);
            Assert.Equal(Operation.ToBeSent, OperationUtils.Parse(outMessage.Operation));
            Assert.Equal(MessageExchangePattern.Push, MessageExchangePatternUtils.Parse(outMessage.MEP));
            Assert.Equal(sendingPMode.MessagePackaging.Mpc, outMessage.Mpc);

        }

        private string RetrievePullingUrlFromConfig(IConfig as4Configuration)
        {
            var pullReceiveAgent = as4Configuration.GetAgentsConfiguration().FirstOrDefault(a => a.Type == AgentType.PullReceive);

            if (pullReceiveAgent == null)
            {
                throw new ConfigurationErrorsException("There is no PullReceive Agent configured.");
            }

            string pmodeId = pullReceiveAgent.Settings.Receiver.Setting.First().Key;

            var pmode = _as4Msh.GetConfiguration().GetSendingPMode(pmodeId);

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
                as4Message.MessageUnits.Add(mu);
            }

            // Add a PMode Id to the primary usermessage, just to be sure that it can be picked up with
            // and processed with a specific receiving PMode.
            if (as4Message.PrimaryUserMessage != null)
            {
                as4Message.PrimaryUserMessage.CollaborationInfo = new CollaborationInfo
                {
                    AgreementReference = new AgreementReference(pmodeId)
                };
            }

            return as4Message;
        }
    }
}

