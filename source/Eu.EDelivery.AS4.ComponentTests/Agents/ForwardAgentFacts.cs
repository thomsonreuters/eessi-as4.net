using System;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Services;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardAgentFacts"/> class.
        /// </summary>
        public ForwardAgentFacts()
        {
            OverrideSettings("forwardagent_settings.xml");
            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);
            _databaseSpy = new DatabaseSpy(_as4Msh.GetConfiguration());
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
            
            // Act
            InsertToBeForwardedInMessage(
                pmodeId: "Forward_Push",
                mep: MessageExchangePattern.Push,
                tobeForwarded: as4Message);

            // Assert: if an OutMessage is created with the correct status and operation.
            InMessage inMessage = await PollUntilPresent(
                () => _databaseSpy.GetInMessageFor(
                    m => m.EbmsMessageId == messageId
                         && m.Operation == Operation.Forwarded),
                TimeSpan.FromSeconds(15));
            Assert.NotNull(AS4XmlSerializer.FromString<ReceivingProcessingMode>(inMessage.PMode));

            OutMessage outMessage = await PollUntilPresent(
                () => _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == messageId),
                timeout: TimeSpan.FromSeconds(5));

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

            // Act
            InsertToBeForwardedInMessage(
                pmodeId: "Forward_Push",
                mep: MessageExchangePattern.Push,
                tobeForwarded: as4Message);

            // Assert: if an OutMessage is created with the correct status and operation.
            InMessage primaryInMessage = await PollUntilPresent(
                () => _databaseSpy.GetInMessageFor(
                    m => m.EbmsMessageId == primaryMessageId
                         && m.Operation == Operation.Forwarded),
                TimeSpan.FromSeconds(15));
            Assert.NotNull(AS4XmlSerializer.FromString<ReceivingProcessingMode>(primaryInMessage.PMode));

            await PollUntilPresent(
                () => _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == secondMessageId),
                timeout: TimeSpan.FromSeconds(5));

            Assert.Equal(Operation.Forwarded, primaryInMessage.Operation);
            Assert.NotNull(AS4XmlSerializer.FromString<ReceivingProcessingMode>(primaryInMessage.PMode));

            OutMessage primaryOutMessage = await PollUntilPresent(
                () => _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == primaryMessageId),
                timeout: TimeSpan.FromSeconds(5));

            Assert.Equal(Operation.ToBeProcessed, primaryOutMessage.Operation);
            Assert.NotNull(AS4XmlSerializer.FromString<SendingProcessingMode>(primaryOutMessage.PMode));

            OutMessage secondaryOutMessage = _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == secondMessageId);
            Assert.Null(secondaryOutMessage);
        }

        [Fact]
        public async Task ForwardingWithPullOnPush()
        {
            // Arrange
            const string messageId = "message-id";
            AS4Message as4Message = AS4Message.Create(CreateForwardPullUserMessage(messageId));

            // Act
            InsertToBeForwardedInMessage(
                pmodeId: "Forward_Pull",
                mep: MessageExchangePattern.Push,
                tobeForwarded: as4Message);

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

        [Fact]
        public async Task ForwardingWithPushOnPull()
        {
            // Arrange
            const string messageId = "user-message-id";
            var tobeForwarded = AS4Message.Create(CreateForwardPushUserMessage(messageId));

            // Act
            InsertToBeForwardedInMessage(
                pmodeId: "Forward_Push", 
                mep: MessageExchangePattern.Pull, 
                tobeForwarded: tobeForwarded);

            // Assert: if an OutMessage is created with the correct status and operation
            await PollUntilPresent(
                () => _databaseSpy.GetInMessageFor(m => 
                    m.EbmsMessageId == messageId 
                    && m.Operation == Operation.Forwarded), 
                TimeSpan.FromSeconds(20));

            OutMessage outMessage = await PollUntilPresent(
                () => _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == messageId),
                TimeSpan.FromSeconds(5));

            var sendingPMode = AS4XmlSerializer.FromString<SendingProcessingMode>(outMessage.PMode);
            Assert.NotNull(sendingPMode);
            Assert.Equal(Operation.ToBeProcessed, outMessage.Operation);
            Assert.Equal(MessageExchangePattern.Push, outMessage.MEP);
            Assert.Equal(sendingPMode.MessagePackaging.Mpc, outMessage.Mpc);
        }

        private void InsertToBeForwardedInMessage(string pmodeId, MessageExchangePattern mep, AS4Message tobeForwarded)
        {
            foreach (MessageUnit m in tobeForwarded.MessageUnits)
            {
                string location =
                    Registry.Instance
                            .MessageBodyStore
                            .SaveAS4Message(
                                _as4Msh.GetConfiguration().InMessageStoreLocation,
                                tobeForwarded);

                var inMessage = new InMessage(m.MessageId)
                {
                    Intermediary = true,
                    Operation = 
                        m.MessageId == tobeForwarded.PrimaryMessageUnit.MessageId 
                            ? Operation.ToBeForwarded 
                            : Operation.NotApplicable,
                    MessageLocation = location,
                    MEP = mep,
                    ContentType = tobeForwarded.ContentType
                };

                ReceivingProcessingMode forwardPMode =
                    _as4Msh.GetConfiguration()
                           .GetReceivingPModes()
                           .First(p => p.Id == pmodeId);

                inMessage.SetPModeInformation(forwardPMode);
                inMessage.SetStatus(InStatus.Received);
                inMessage.AssignAS4Properties(m);
                _databaseSpy.InsertInMessage(inMessage);
            }
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

