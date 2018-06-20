using System.Linq;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Core
{
    /// <summary>
    /// Internal Message Builder to create an <see cref="MessagingContext" />
    /// </summary>    
    public class MessageContextBuilder
    {
        private readonly MessagingContext _messagingContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageContextBuilder"/> class.
        /// </summary>
        /// <param name="messageId">The message Id.</param>
        public MessageContextBuilder(string messageId = "message-id")
        {
            _messagingContext = new MessagingContext(AS4Message.Empty, MessagingContextMode.Receive);

            UserMessage userMessage = CreateDefaultUserMessage(messageId);
            _messagingContext.AS4Message.AddMessageUnit(userMessage);
        }

        /// <summary>
        /// Build to the Builder
        /// </summary>
        /// <returns></returns>
        public MessagingContext Build()
        {
            return _messagingContext;
        }

        /// <summary>
        /// Add Agreement Reference to the Builder
        /// </summary>
        /// <param name="agreementRef"></param>
        /// <returns></returns>
        public MessageContextBuilder WithAgreementRef(AgreementReference agreementRef)
        {
            UserMessage userMessage = _messagingContext.AS4Message.FirstUserMessage;
            userMessage.CollaborationInfo.AgreementReference = agreementRef;

            return this;
        }

        /// <summary>
        /// Add Parties to the Builder
        /// </summary>
        /// <param name="fromParty"></param>
        /// <param name="toParty"></param>
        /// <returns></returns>
        public MessageContextBuilder WithPartys(Party fromParty, Party toParty)
        {
            UserMessage userMessage = _messagingContext.AS4Message.UserMessages.First();
            userMessage.Sender = fromParty;
            userMessage.Receiver = toParty;

            return this;
        }

        /// <summary>
        /// Add a PMode Id to the Builder
        /// </summary>
        /// <param name="pmodeId"></param>
        /// <returns></returns>
        public MessageContextBuilder WithPModeId(string pmodeId)
        {
            _messagingContext.AS4Message.UserMessages.First().CollaborationInfo.AgreementReference.PModeId = pmodeId;

            return this;
        }

        /// <summary>
        /// Add Service/Action to the Builder
        /// </summary>
        /// <param name="service"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public MessageContextBuilder WithServiceAction(string service, string action)
        {
            UserMessage userMessage = _messagingContext.AS4Message.UserMessages.First();
            userMessage.CollaborationInfo.Action = action;
            userMessage.CollaborationInfo.Service.Value = service;

            return this;
        }

        /// <summary>
        /// Add a SignalMessage to the Builder
        /// </summary>
        /// <param name="signalMessage"></param>
        /// <returns></returns>
        public MessageContextBuilder WithSignalMessage(SignalMessage signalMessage)
        {
            _messagingContext.AS4Message.AddMessageUnit(signalMessage);

            return this;
        }

        /// <summary>
        /// Add a UserMessage to the Builder
        /// </summary>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        public MessageContextBuilder WithUserMessage(UserMessage userMessage)
        {
            _messagingContext.AS4Message.AddMessageUnit(userMessage);

            return this;
        }

        /// <summary>
        /// Add a <see cref="SendingProcessingMode"/> to the Builder.
        /// </summary>
        /// <param name="pmode">The given pmode.</param>
        /// <returns></returns>
        public MessageContextBuilder WithSendingPMode(SendingProcessingMode pmode)
        {
            _messagingContext.SendingPMode = pmode;
            return this;
        }

        /// <summary>
        /// Add a <see cref="ReceivingProcessingMode"/> to the Builder.
        /// </summary>
        /// <param name="pmode">The given pmode.</param>
        /// <returns></returns>
        public MessageContextBuilder WithReceivingPMode(ReceivingProcessingMode pmode)
        {
            _messagingContext.ReceivingPMode = pmode;
            return this;
        }

        private static UserMessage CreateDefaultUserMessage(string messageId)
        {
            var userMessage = new UserMessage
            {
                CollaborationInfo = { AgreementReference = new AgreementReference() },
                MessageId = messageId
            };

            return userMessage;
        }
    }
}