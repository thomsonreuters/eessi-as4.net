using System;
using System.IO;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;

namespace Eu.EDelivery.AS4.Builders.Entities
{
    /// <summary>
    /// Builder to create <see cref="InMessage"/> Models
    /// </summary>
    internal class InMessageBuilder
    {
        private readonly MessageUnit _messageUnit;
        private readonly string _contentType;
        private readonly MessageExchangePattern _mep;

        private SendingProcessingMode _pmode;
        private string _location;

        private InMessageBuilder(MessageUnit messageUnit, string contentType, MessageExchangePattern mep)
        {
            _messageUnit = messageUnit;
            _contentType = contentType;
            _mep = mep;
        }

        /// <summary>
        /// Creates a new InMessageBuilder instance that can instantiate an <see cref="InMessage"/> for the received <paramref name="userMessage"/>
        /// </summary>
        /// <param name="userMessage">The UserMessage for which an InMessage must be created.</param>
        /// <param name="belongsToAS4Message">The AS4Message that contains the <paramref name="userMessage"/></param>
        /// <param name="mep"></param>
        /// <returns></returns>
        public static InMessageBuilder ForUserMessage(UserMessage userMessage, AS4Message belongsToAS4Message, MessageExchangePattern mep)
        {
            if (userMessage == null)
            {
                throw new ArgumentNullException(nameof(userMessage));
            }
            if (belongsToAS4Message == null)
            {
                throw new ArgumentNullException(nameof(belongsToAS4Message));
            }

            return new InMessageBuilder(userMessage, belongsToAS4Message.ContentType, mep);
        }

        /// <summary>
        /// Creates a new InMessageBuilder instance that can instantiate an <see cref="InMessage"/> for the received <paramref name="signalMessage"/>
        /// </summary>
        /// <param name="signalMessage"></param>
        /// <param name="belongsToAS4Message"></param>
        /// <param name="mep"></param>
        /// <returns></returns>
        public static InMessageBuilder ForSignalMessage(SignalMessage signalMessage, AS4Message belongsToAS4Message, MessageExchangePattern mep)
        {
            if (signalMessage == null)
            {
                throw new ArgumentNullException(nameof(signalMessage));
            }

            if (belongsToAS4Message == null)
            {
                throw new ArgumentNullException(nameof(belongsToAS4Message));
            }

            return new InMessageBuilder(signalMessage, belongsToAS4Message.ContentType, mep);
        }

        /// <summary>
        /// Assign sending/responding information about the message.
        /// </summary>
        /// <param name="pmode"></param>
        /// <returns></returns>
        public InMessageBuilder WithPMode(SendingProcessingMode pmode)
        {
            _pmode = pmode;
            return this;
        }

        /// <summary>
        /// Assign the location to where the <see cref="AS4Message"/> is stored.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public InMessageBuilder OnLocation(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(location));
            }

            _location = location;
            return this;
        }

        /// <summary>
        /// Prepare <see cref="InMessage"/> as an Error.
        /// This is used when the ednpoint of the message has been determined and not processing should be done.
        /// </summary>
        /// <returns></returns>
        public InMessage BuildAsError()
        {
            InMessage inMessage = BuildYetUndetermined();
            inMessage.Operation =
                (_pmode?.ErrorHandling?.NotifyMessageProducer ?? false)
                    ? Operation.ToBeNotified
                    : Operation.NotApplicable;

            return inMessage;
        }

        /// <summary>
        /// Prepare <see cref="InMessage"/> as a message that has yet to be determined what it's endpoint will be.
        /// This is used for (quick) saving the incoming message but process the message on a later time.
        /// </summary>
        /// <returns></returns>
        public InMessage BuildYetUndetermined()
        {
            if (_messageUnit == null)
            {
                throw new InvalidDataException("Builder needs a Message Unit for building an InMessage");
            }

            var inMessage = new InMessage(_messageUnit.MessageId)
            {
                EbmsRefToMessageId = _messageUnit.RefToMessageId,
                ContentType = _contentType,
                InsertionTime = DateTimeOffset.Now,
                ModificationTime = DateTimeOffset.Now,
                MessageLocation = _location,
                EbmsMessageType = DetermineMessageType(_messageUnit),
                MEP = _mep,
                Operation = Operation.NotApplicable
            };

            inMessage.SetPModeInformation(_pmode);
            inMessage.SetStatus(InStatus.Received);
            inMessage.AssignAS4Properties(_messageUnit);

            return inMessage;
        }

        private static MessageType DetermineMessageType(MessageUnit messageUnit)
        {
            switch (messageUnit)
            {
                case UserMessage _: return MessageType.UserMessage;
                case Receipt _: return MessageType.Receipt;
                case Error _: return MessageType.Error;
            }

            throw new InvalidOperationException("There is no MessageType mapped for this MessageUnit.");
        }
    }
}
