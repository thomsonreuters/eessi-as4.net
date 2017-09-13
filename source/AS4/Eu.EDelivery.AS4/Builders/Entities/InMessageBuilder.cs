using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;

namespace Eu.EDelivery.AS4.Builders.Entities
{
    /// <summary>
    /// Builder to create <see cref="InMessage"/> Models
    /// </summary>
    public class InMessageBuilder
    {
        private readonly MessageUnit _messageUnit;
        private readonly string _contentType;
        private readonly MessageExchangePattern _mep;
        private IPMode _pmode;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMessageBuilder" /> class.
        /// Starting the Builder with a given Serialize Provider
        /// </summary>
        /// <param name="messageUnit">The message unit.</param>
        /// <param name="contentType">The contentType of the AS4Message Body to which the MessageUnit belongs to</param>
        /// <param name="mep"><see cref="MessageExchangePattern"/> that describes how the Message was received.</param>
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
            return new InMessageBuilder(signalMessage, belongsToAS4Message.ContentType, mep);
        }

        public InMessageBuilder WithPMode(IPMode pmode)
        {
            _pmode = pmode;
            return this;
        }

        /// <summary>
        /// Start Creating the <see cref="InMessage"/>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<InMessage> BuildAsync(CancellationToken cancellationToken)
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
                ModificationTime = DateTimeOffset.Now
            };

            inMessage.SetEbmsMessageType(DetermineMessageType(_messageUnit));
            inMessage.SetMessageExchangePattern(_mep);
            inMessage.SetOperation(Operation.NotApplicable);
            await inMessage.SetPModeInformationAsync(_pmode);
            inMessage.SetStatus(InStatus.Received);

            inMessage.AssignAS4Properties(_messageUnit, cancellationToken);

            return inMessage;
        }

        private static MessageType DetermineMessageType(MessageUnit messageUnit)
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

            throw new InvalidOperationException("There is no MessageType mapped for this MessageUnit.");
        }
    }
}
