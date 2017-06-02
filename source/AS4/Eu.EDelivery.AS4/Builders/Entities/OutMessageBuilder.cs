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
        private readonly MessageUnit _messageUnitUnit;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="OutMessageBuilder"/> class.         
        /// </summary>
        /// <summary>        
        /// </summary>
        /// <param name="messageUnit">The <see cref="MessageUnit"/> for which an <see cref="OutMessage"/> should be created.</param>
        /// <param name="as4Message">The <see cref="AS4Message"/> to which the <paramref name="messageUnit"/> belongs to.</param>
        private OutMessageBuilder(MessageUnit messageUnit, AS4Message as4Message)
        {
            _as4Message = as4Message;
            _messageUnitUnit = messageUnit;
        }

        /// <summary>
        /// Creates an OutMessageBuilder for the specified <paramref name="message"/>.
        /// </summary>
        /// <param name="messageUnit">The <see cref="MessageUnit"/> for which an <see cref="OutMessage"/> should be created.</param>
        /// <param name="message">The <see cref="AS4Message"/> to which the <paramref name="messageUnit"/> belongs to.</param>
        /// <returns></returns>
        public static OutMessageBuilder ForAS4Message(MessageUnit messageUnit, AS4Message message)
        {
            var builder = new OutMessageBuilder(messageUnit, message);
            return builder;
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
            string messageId = _messageUnitUnit.MessageId;

            OutMessage outMessage = CreateDefaultOutMessage(messageId);
            outMessage.ContentType = _as4Message.ContentType;
            outMessage.Message = _as4Message;
            outMessage.EbmsMessageType = DetermineSignalMessageType(_messageUnitUnit);
            outMessage.PMode = AS4XmlSerializer.ToString(GetSendingPMode(outMessage.EbmsMessageType));
            
            if (String.IsNullOrWhiteSpace(_messageUnitUnit.RefToMessageId) == false)
            {
                outMessage.EbmsRefToMessageId = _messageUnitUnit.RefToMessageId;
            }

            return outMessage;
        }

        private SendingProcessingMode GetSendingPMode(MessageType messageType)
        {
            bool isSendPModeNotFound = _as4Message.SendingPMode?.Id == null;
            ReceivingProcessingMode receivePMode = _as4Message.ReceivingPMode;
           
            if (isSendPModeNotFound && messageType == MessageType.Receipt && receivePMode?.ReceiptHandling.ReplyPattern == ReplyPattern.Callback)
            {
                return Config.Instance.GetSendingPMode(receivePMode.ReceiptHandling.SendingPMode);
            }

            if (isSendPModeNotFound && messageType == MessageType.Error && receivePMode?.ErrorHandling.ReplyPattern == ReplyPattern.Callback)
            {
                return Config.Instance.GetSendingPMode(receivePMode.ErrorHandling.SendingPMode);
            }

            return _as4Message.SendingPMode;
        }

        private static MessageType DetermineSignalMessageType(MessageUnit messageUnit)
        {
            if (messageUnit is UserMessage)
            {
                return MessageType.UserMessage;
            }

            if (messageUnit is Receipt)
            {
                return MessageType.Receipt;
            }

            if (messageUnit is Error)
            {
                return MessageType.Error;
            }

            throw new NotSupportedException($"There exists no MessageType mapping for the specified MessageUnit type {typeof(MessageUnit)}");
        }
       
        private OutMessage CreateDefaultOutMessage(string messageId)
        {
            return new OutMessage
            {
                EbmsMessageId = messageId,
                ContentType = _as4Message.ContentType,                
                Operation = Operation.NotApplicable,
                ModificationTime = DateTimeOffset.Now,
                InsertionTime = DateTimeOffset.Now
            };
        }        
    }
}
