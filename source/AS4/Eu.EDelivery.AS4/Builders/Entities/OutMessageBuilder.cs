using System;
using System.Linq;
using System.Threading;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Builders.Entities
{
    /// <summary>
    /// Builder to create <see cref="OutMessage"/> Models
    /// </summary>
    public class OutMessageBuilder
    {
        private readonly MessageUnit _messageUnitUnit;
        private readonly MessagingContext _messagingContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutMessageBuilder" /> class.
        /// </summary>
        /// <param name="messageUnit">The message unit.</param>
        /// <param name="message">The message.</param>
        public OutMessageBuilder(MessageUnit messageUnit, MessagingContext message)
        {
            _messageUnitUnit = messageUnit;
            _messagingContext = message;
        }

        /// <summary>
        /// For a given <paramref name="messageUnit"/>.
        /// </summary>
        /// <param name="messageUnit">The message unit.</param>
        /// <param name="context">The messaging context.</param>
        /// <returns></returns>
        public static OutMessageBuilder ForMessageUnit(MessageUnit messageUnit, MessagingContext context)
        {
            return new OutMessageBuilder(messageUnit, context);
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
            outMessage.ContentType = _messagingContext.AS4Message.ContentType;
            outMessage.Message = _messagingContext.AS4Message;
            outMessage.EbmsMessageType = DetermineSignalMessageType(_messageUnitUnit);
            outMessage.PMode = AS4XmlSerializer.ToString(GetSendingPMode(outMessage.EbmsMessageType));
            
            if (string.IsNullOrWhiteSpace(_messageUnitUnit.RefToMessageId) == false)
            {
                outMessage.EbmsRefToMessageId = _messageUnitUnit.RefToMessageId;
            }

            UserMessage userMessage = _messagingContext.AS4Message.PrimaryUserMessage;

            if (userMessage != null)
            {
                outMessage.FromParty = userMessage.Sender.PartyIds.First().Id;
                outMessage.ToParty = userMessage.Receiver.PartyIds.First().Id;
                outMessage.ConversationId = userMessage.CollaborationInfo.ConversationId;
                outMessage.Action = userMessage.CollaborationInfo.Action;
                outMessage.Service = userMessage.CollaborationInfo.Service.Value;
                outMessage.IsDuplicate = userMessage.IsDuplicate;
                outMessage.IsTest = userMessage.IsTest;
                outMessage.Mpc = userMessage.Mpc;
                outMessage.SoapEnvelope = AS4XmlSerializer.ToDocument(_messagingContext, cancellationToken).OuterXml;
            }

            return outMessage;
        }

        private SendingProcessingMode GetSendingPMode(MessageType messageType)
        {
            bool isSendPModeNotFound = _messagingContext.SendingPMode?.Id == null;
            ReceivingProcessingMode receivePMode = _messagingContext.ReceivingPMode;
           
            if (isSendPModeNotFound && messageType == MessageType.Receipt && receivePMode?.ReceiptHandling.ReplyPattern == ReplyPattern.Callback)
            {
                return Config.Instance.GetSendingPMode(receivePMode.ReceiptHandling.SendingPMode);
            }

            if (isSendPModeNotFound && messageType == MessageType.Error && receivePMode?.ErrorHandling.ReplyPattern == ReplyPattern.Callback)
            {
                return Config.Instance.GetSendingPMode(receivePMode.ErrorHandling.SendingPMode);
            }

            return _messagingContext.SendingPMode;
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
                ContentType = _messagingContext.AS4Message.ContentType,                
                Operation = Operation.NotApplicable,
                ModificationTime = DateTimeOffset.Now,
                InsertionTime = DateTimeOffset.Now
            };
        }        
    }
}
