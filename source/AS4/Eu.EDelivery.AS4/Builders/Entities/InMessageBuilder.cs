using System;
using System.IO;
using System.Threading;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Builders.Entities
{
    /// <summary>
    /// Builder to create <see cref="InMessage"/> Models
    /// </summary>
    public class InMessageBuilder
    {
        private readonly ISerializerProvider _provider;

        private MessageUnit _messageUnit;
        private AS4Message _as4Message;
        private string _pmodeString;
        private MessageType _messageType;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMessageBuilder"/> class. 
        /// Starting the Builder
        /// </summary>
        public InMessageBuilder()
        {
            _provider = new SerializerProvider();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMessageBuilder"/> class. 
        /// Starting the Builder with a given Serialize Provider
        /// </summary>
        /// <param name="provider">
        /// </param>
        public InMessageBuilder(ISerializerProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Add a <see cref="AS4Message"/> Body
        /// to the Builder
        /// </summary>
        /// <param name="as4Message"></param>
        /// <returns></returns>
        public InMessageBuilder WithAS4Message(AS4Message as4Message)
        {
            _as4Message = as4Message;
            return this;
        }

        /// <summary>
        /// Add a SignalMessage to the Root
        /// of the Builder
        /// </summary>
        /// <param name="messageUnit"></param>
        /// <returns></returns>
        public InMessageBuilder WithMessageUnit(MessageUnit messageUnit)
        {
            _messageUnit = messageUnit;
            return this;
        }

        /// <summary>
        /// Add the Message Type
        /// to the Builder
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public InMessageBuilder WithEbmsMessageType(MessageType messageType)
        {
            _messageType = messageType;
            return this;
        }

        public InMessageBuilder WithPModeString(string pmode)
        {
            _pmodeString = pmode;
            return this;
        }

        /// <summary>
        /// Start Creating the <see cref="InMessage"/>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public InMessage Build(CancellationToken cancellationToken)
        {
            if (_as4Message == null)
            {
                throw new AS4Exception("Builder needs a AS4Message for building an InMessage");
            }
            if (_messageUnit == null)
            {
                throw new AS4Exception("Builder needs a Message Unit for building an InMessage");
            }

            return new InMessage
            {
                EbmsMessageId = this._messageUnit.MessageId,
                EbmsRefToMessageId = this._messageUnit.RefToMessageId,
                EbmsMessageType = this._messageType,
                MessageBody = CreateMessageBody(this._as4Message, cancellationToken),
                ContentType = this._as4Message.ContentType,
                PMode = this._pmodeString,
                MEP = MessageExchangePattern.Push,
                Status = InStatus.Received,
                Operation = Operation.NotApplicable,
                InsertionTime = DateTimeOffset.UtcNow,
                ModificationTime = DateTimeOffset.UtcNow
            };
        }

        private byte[] CreateMessageBody(AS4Message as4Message, CancellationToken token)
        {
            var memoryStream = new MemoryStream();
            ISerializer serializer = this._provider.Get(as4Message.ContentType);
            serializer.Serialize(as4Message, memoryStream, token);

            return memoryStream.ToArray();
        }
    }
}
