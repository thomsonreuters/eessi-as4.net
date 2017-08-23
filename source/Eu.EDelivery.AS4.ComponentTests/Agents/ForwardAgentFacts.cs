using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;

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
                AgentSettings receivingAgent =
                    as4Component.GetConfiguration().GetSettingsAgents().FirstOrDefault(a => a.Name.Equals("Receive Agent"));

                Assert.True(receivingAgent != null, "The Agent with name Receive Agent could not be found");

                return receivingAgent.Receiver?.Setting?.FirstOrDefault(s => s.Key == "Url")?.Value;
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
            var as4Message = CreateAS4Message(new UserMessage(messageId));

            var response = await StubSender.SendAS4Message(_receiveAgentUrl, as4Message);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            await Task.Delay(TimeSpan.FromSeconds(3));

            // Assert if an OutMessage is created with the correct status and operation.
            var inMessage = _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == messageId);
            var outMessage = _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == messageId);

            Assert.NotNull(inMessage);
            Assert.Equal(Operation.Forwarded, inMessage.Operation);
            Assert.NotNull(AS4XmlSerializer.FromString<ReceivingProcessingMode>(inMessage.PMode));

            Assert.NotNull(outMessage);
            Assert.Equal(Operation.ToBeSent, outMessage.Operation);
            Assert.NotNull(AS4XmlSerializer.FromString<SendingProcessingMode>(outMessage.PMode));
        }

        [Fact]
        public async Task OutMessageIsCreatedForPrimaryMessageUnitOfToBeForwardedAS4Message()
        {
            const string messageId = "primary-message-id";
            const string secondMessageId = "secondary-message-id";

            // Send an AS4Message to the AS4 MSH which has a receive-agent configured.
            var as4Message = CreateAS4Message(new UserMessage(messageId), new UserMessage(secondMessageId));

            var response = await StubSender.SendAS4Message(_receiveAgentUrl, as4Message);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            await Task.Delay(TimeSpan.FromSeconds(3));

            // Assert if an OutMessage is created with the correct status and operation.
            var primaryInMessage = _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == messageId);
            var secondaryInMessage = _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == secondMessageId);
            var primaryOutMessage = _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == messageId);
            var secondaryOutMessage = _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == secondMessageId);

            Assert.NotNull(primaryInMessage);
            Assert.Equal(Operation.Forwarded, primaryInMessage.Operation);
            Assert.NotNull(AS4XmlSerializer.FromString<ReceivingProcessingMode>(primaryInMessage.PMode));

            Assert.NotNull(secondaryInMessage);
            Assert.Equal(Operation.Forwarded, primaryInMessage.Operation);
            Assert.NotNull(AS4XmlSerializer.FromString<ReceivingProcessingMode>(primaryInMessage.PMode));

            Assert.NotNull(primaryOutMessage);
            Assert.Equal(Operation.ToBeSent, primaryOutMessage.Operation);
            Assert.NotNull(AS4XmlSerializer.FromString<SendingProcessingMode>(primaryOutMessage.PMode));

            Assert.Null(secondaryOutMessage);
        }

        private AS4Message CreateAS4Message(params MessageUnit[] messageUnits)
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
                    AgreementReference = new AgreementReference("ComponentTest_ReceiveAgent_Forward")
                };
            }

            return as4Message;
        }
    }
}

