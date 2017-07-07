using System;
using System.Threading;
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
        private readonly MessageUnit _messageUnit;
        private readonly AS4Message _belongsToAS4Message;

        private SendingProcessingMode _sendingProcessingMode;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutMessageBuilder" /> class.
        /// </summary>
        /// <param name="messageUnit">The message unit.</param>
        /// <param name="belongsToAS4Message">The AS4 message to which the <paramref name="messageUnit"/> belongs to.</param>
        private OutMessageBuilder(MessageUnit messageUnit, AS4Message belongsToAS4Message)
        {
            _messageUnit = messageUnit;
            _belongsToAS4Message = belongsToAS4Message;
        }

        /// <summary>
        /// For a given <paramref name="messageUnit"/>.
        /// </summary>
        /// <param name="messageUnit">The message unit.</param>
        /// <param name="belongsToAS4Message">The AS4 Message to which the <paramref name="messageUnit"/> belongs to.</param>
        /// <returns></returns>
        public static OutMessageBuilder ForMessageUnit(MessageUnit messageUnit, AS4Message belongsToAS4Message)
        {
            return new OutMessageBuilder(messageUnit, belongsToAS4Message);
        }

        public OutMessageBuilder WithSendingPMode(SendingProcessingMode pmode)
        {
            _sendingProcessingMode = pmode;
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
            MessageType messageType = DetermineSignalMessageType(_messageUnit);

            var outMessage = new OutMessage
            {
                EbmsMessageId = _messageUnit.MessageId,
                ContentType = _belongsToAS4Message.ContentType,
                Operation = Operation.NotApplicable,
                ModificationTime = DateTimeOffset.Now,
                InsertionTime = DateTimeOffset.Now,
                MEP = _belongsToAS4Message.Mep,
                EbmsMessageType = messageType,
                PMode = AS4XmlSerializer.ToString(_sendingProcessingMode),
            };

            if (string.IsNullOrWhiteSpace(_messageUnit.RefToMessageId) == false)
            {
                outMessage.EbmsRefToMessageId = _messageUnit.RefToMessageId;
            }
            
            outMessage.AssignAS4Properties(_messageUnit, cancellationToken);

            return outMessage;
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
    }
}
