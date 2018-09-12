using System;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;

namespace Eu.EDelivery.AS4.Builders.Entities
{
    /// <summary>
    /// Builder to create <see cref="OutMessage"/> Models
    /// </summary>
    internal class OutMessageBuilder
    {
        private readonly MessageUnit _messageUnit;
        private readonly string _contentType;
        private readonly SendingProcessingMode _sendingProcessingMode;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutMessageBuilder" /> class.
        /// </summary>
        /// <param name="messageUnit">The message unit.</param>
        /// <param name="contentType">The ContentType of the Message</param>        
        /// <param name="sendingPMode">The Sending PMode that must is used to send this message</param>
        private OutMessageBuilder(MessageUnit messageUnit, string contentType, SendingProcessingMode sendingPMode)
        {
            _messageUnit = messageUnit;
            _contentType = contentType;
            _sendingProcessingMode = sendingPMode;
        }

        /// <summary>
        /// For a given <paramref name="messageUnit"/>.
        /// </summary>
        /// <param name="messageUnit">The message unit.</param>
        /// <param name="contentType"></param>
        /// <param name="sendingPMode">The Sending PMode that is used for this message</param>
        /// <returns></returns>
        public static OutMessageBuilder ForMessageUnit(MessageUnit messageUnit, string contentType, SendingProcessingMode sendingPMode)
        {
            return new OutMessageBuilder(messageUnit, contentType, sendingPMode);
        }

        /// <summary>
        /// Start Creating the <see cref="OutMessage"/>
        /// </summary>
        /// <returns>
        /// </returns>
        public OutMessage Build()
        {
            MessageType messageType = DetermineSignalMessageType(_messageUnit);

            var outMessage = new OutMessage(_messageUnit.MessageId)
            {
                ContentType = _contentType,
                ModificationTime = DateTimeOffset.Now,
                InsertionTime = DateTimeOffset.Now,
                Operation = Operation.NotApplicable,
                MEP = DetermineMepOf(_sendingProcessingMode),
                EbmsMessageType = messageType
            };

            outMessage.SetPModeInformation(_sendingProcessingMode);

            if (string.IsNullOrWhiteSpace(_messageUnit.RefToMessageId) == false)
            {
                outMessage.EbmsRefToMessageId = _messageUnit.RefToMessageId;
            }

            outMessage.AssignAS4Properties(_messageUnit);

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

        private static MessageExchangePattern DetermineMepOf(SendingProcessingMode pmode)
        {
            switch (pmode?.MepBinding)
            {
                case MessageExchangePatternBinding.Pull:
                    return MessageExchangePattern.Pull;
                default:
                    return MessageExchangePattern.Push;
            }
        }
    }
}
