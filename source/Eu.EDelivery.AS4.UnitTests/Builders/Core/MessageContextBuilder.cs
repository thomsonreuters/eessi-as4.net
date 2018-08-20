using System;
using System.Linq;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using AgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using Service = Eu.EDelivery.AS4.Model.Core.Service;

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

            UserMessage userMessage = new UserMessage(
                messageId,
                new CollaborationInfo(new AgreementReference(Guid.NewGuid().ToString())));
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
    }
}