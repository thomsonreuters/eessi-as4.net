using System;
using System.IO;
using System.Threading;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
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
        private readonly ISerializerProvider _provider;

        private string _messageId;
        private AS4Message _as4Message;
        private MessageType _messageType;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutMessageBuilder"/> class. 
        /// Start Builder with default settings
        /// </summary>
        public OutMessageBuilder()
        {
            _provider = new SerializerProvider();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutMessageBuilder"/> class. 
        /// Start Builder with given Serializer Provider
        /// </summary>
        /// <param name="provider">
        /// </param>
        public OutMessageBuilder(ISerializerProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Add a <see cref="AS4Message"/> Body
        /// to the Builder
        /// </summary>
        /// <param name="as4Message"></param>
        /// <returns></returns>
        public OutMessageBuilder WithAS4Message(AS4Message as4Message)
        {
            _as4Message = as4Message;
            return this;
        }

        /// <summary>
        /// Add a SignalMessage to the Root
        /// of the Builder
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public OutMessageBuilder WithEbmsMessageId(string messageId)
        {
            _messageId = messageId;
            return this;
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
            if (_as4Message == null)
            {
                throw new AS4Exception("Builder needs an AS4Message for building a OutMessage");
            }

            OutMessage outMessage = CreateDefaultOutMessage();

            AddMessageBodyToMessage(outMessage, cancellationToken);
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

        private OutMessage CreateDefaultOutMessage()
        {
            return new OutMessage
            {
                EbmsMessageId = _messageId,
                ContentType = _as4Message.ContentType,
                EbmsMessageType = _messageType,
                Operation = Operation.NotApplicable,                
                ModificationTime = DateTimeOffset.Now,
                InsertionTime = DateTimeOffset.Now
            };
        }

        private void AddMessageBodyToMessage(MessageEntity messageEntity, CancellationToken token)
        {
            ISerializer serializer = _provider.Get(_as4Message.ContentType);
            using (var outputStream = new MemoryStream())
            {
                serializer.Serialize(_as4Message, outputStream, token);

                messageEntity.MessageBody = outputStream.ToArray();
                messageEntity.ContentType = _as4Message.ContentType;
            }
           
            CloseAttachmentContentStreams();
        }

        private void CloseAttachmentContentStreams()
        {
            foreach (Attachment attachment in _as4Message.Attachments)
            {
                attachment.Content.Close();
            }
        }
    }
}
