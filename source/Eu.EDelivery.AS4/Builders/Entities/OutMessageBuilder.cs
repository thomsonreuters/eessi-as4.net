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
        private readonly IPMode _pmode;

        private OutMessageBuilder(MessageUnit messageUnit, string contentType, IPMode pmode)
        {
            _messageUnit = messageUnit;
            _contentType = contentType;
            _pmode = pmode;
        }

        /// <summary>
        /// For a given <paramref name="messageUnit"/>.
        /// </summary>
        /// <param name="messageUnit">The message unit.</param>
        /// <param name="contentType"></param>
        /// <param name="pmode">The PMode that is used for this message</param>
        /// <returns></returns>
        public static OutMessageBuilder ForMessageUnit(MessageUnit messageUnit, string contentType, IPMode pmode)
        {
            if (messageUnit == null)
            {
                throw new ArgumentNullException(nameof(messageUnit));
            }

            if (String.IsNullOrWhiteSpace(contentType))
            {
                throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(contentType));
            }

            return new OutMessageBuilder(messageUnit, contentType, pmode);
        }

        /// <summary>
        /// Prepare an <see cref="OutMessage"/> to be picked or stored by a Sending operation.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="status"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public OutMessage BuildForSending(
            string location,
            string url,
            OutStatus status,
            Operation operation)
        {
            if (String.IsNullOrWhiteSpace(location))
            {
                throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(location));
            }

            OutMessage outMessage = Build();

            outMessage.Url = url;
            outMessage.MessageLocation = location;
            outMessage.SetStatus(status);
            outMessage.Operation = operation;

            return outMessage;
        }

        /// <summary>
        /// Prepare an <see cref="OutMessage"/> to be picked up by the Forward Agent.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="receivedInMessage"></param>
        /// <returns></returns>
        public OutMessage BuildForForwarding(string location, InMessage receivedInMessage)
        {
            if (String.IsNullOrWhiteSpace(location))
            {
                throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(location));
            }

            if (receivedInMessage == null)
            {
                throw new ArgumentNullException(nameof(receivedInMessage));
            }

            OutMessage outMessage = Build();
            outMessage.MessageLocation = location;
            outMessage.Intermediary = true;
            outMessage.IsDuplicate = receivedInMessage.IsDuplicate;
            outMessage.Mpc = (_pmode as SendingProcessingMode)?.MessagePackaging?.Mpc;
            outMessage.Operation = Operation.ToBeProcessed;

            return outMessage;
        }

        private OutMessage Build()
        {
            var outMessage = new OutMessage(_messageUnit.MessageId)
            {
                ContentType = _contentType,
                ModificationTime = DateTimeOffset.Now,
                InsertionTime = DateTimeOffset.Now,
                Operation = Operation.NotApplicable,
                MEP = DetermineMepOf(_pmode as SendingProcessingMode),
                EbmsMessageType = DetermineSignalMessageType(_messageUnit)
            };

            outMessage.SetPModeInformation(_pmode);
            outMessage.AssignAS4Properties(_messageUnit);

            if (String.IsNullOrWhiteSpace(_messageUnit.RefToMessageId) == false)
            {
                outMessage.EbmsRefToMessageId = _messageUnit.RefToMessageId;
            }

            return outMessage;
        }

        private static MessageType DetermineSignalMessageType(MessageUnit messageUnit)
        {
            switch (messageUnit)
            {
                case UserMessage _: return MessageType.UserMessage;
                case Receipt _: return MessageType.Receipt;
                case Error _: return MessageType.Error;
            }

            throw new NotSupportedException(
                $"There exists no MessageType mapping for the specified MessageUnit type {typeof(MessageUnit)}");
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
