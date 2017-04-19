using System;
using System.Threading;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Builders.Entities
{
    /// <summary>
    /// Builder to create <see cref="OutMessage"/> Models
    /// </summary>
    public class OutMessageBuilder
    {
        private readonly AS4Message _as4Message;

        private MessageType _messageType;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutMessageBuilder"/> class. 
        /// Start Builder with default settings
        /// </summary>
        /// <summary>
        /// Initializes a new instance of the <see cref="OutMessageBuilder"/> class. 
        /// Start Builder with given Serializer Provider
        /// </summary>                
        /// <param name="as4Message">The <see cref="AS4Message"/> for which an OutMessage should be created</param>        
        private OutMessageBuilder(AS4Message as4Message)
        {
            _as4Message = as4Message;
        }

        /// <summary>
        /// Creates an OutMessageBuilder for the specified <paramref name="message"/>.
        /// </summary>
        /// <param name="message"></param>       
        /// <returns></returns>
        public static OutMessageBuilder ForAS4Message(AS4Message message)
        {
            var builder = new OutMessageBuilder(message);
            return builder;
        }

        /// <summary>
        /// Add the Message ExceptionType
        /// to the Builder
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public OutMessageBuilder WithEbmsMessageType(MessageType messageType)
        {
            _messageType = messageType;
            return this;
        }

        /// <summary>
        /// Start Creating the <see cref="OutMessage"/>
        /// </summary>
        /// <param name="cancellationToken">
        /// The cancellation Token.
        /// </param>
        /// <returns>
        /// </returns>
        public OutMessage Build(CancellationToken cancellationToken)
        {
            string messageId = _as4Message.GetPrimaryMessageId();

            OutMessage outMessage = CreateDefaultOutMessage(messageId);
            outMessage.ContentType = _as4Message.ContentType;
            outMessage.Message = _as4Message;
            outMessage.PMode = AS4XmlSerializer.ToString(GetSendingPMode());

            return outMessage;
        }

        private SendingProcessingMode GetSendingPMode()
        {
            bool isSendPModeNotFound = _as4Message.SendingPMode?.Id == null;
            ReceivingProcessingMode receivePMode = _as4Message.ReceivingPMode;
            bool isCallback = receivePMode?.ReceiptHandling.ReplyPattern == ReplyPattern.Callback;

            if (isSendPModeNotFound && _messageType == MessageType.Receipt && isCallback)
            {
                return Config.Instance.GetSendingPMode(receivePMode.ReceiptHandling.SendingPMode);
            }

            if (isSendPModeNotFound && _messageType == MessageType.Error && isCallback)
            {
                return Config.Instance.GetSendingPMode(receivePMode.ErrorHandling.SendingPMode);
            }

            return _as4Message.SendingPMode;
        }

        private OutMessage CreateDefaultOutMessage(string messageId)
        {
            return new OutMessage
            {
                EbmsMessageId = messageId,
                ContentType = _as4Message.ContentType,
                EbmsMessageType = _messageType,
                Operation = Operation.NotApplicable,
                ModificationTime = DateTimeOffset.Now,
                InsertionTime = DateTimeOffset.Now
            };
        }
    }
}
