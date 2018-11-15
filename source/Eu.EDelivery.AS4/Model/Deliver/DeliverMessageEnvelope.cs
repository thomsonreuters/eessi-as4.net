using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Model.Deliver
{
    public class DeliverMessageEnvelope
    {
        private byte[] _alreadySerializedDeliverMessage;

        public string ContentType { get; }

        public IEnumerable<Attachment> Attachments { get; }

        public DeliverMessage Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeliverMessageEnvelope"/> class.
        /// </summary>
        public DeliverMessageEnvelope(
            DeliverMessage message,
            string contentType,
            IEnumerable<Attachment> attachments)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            if (attachments == null)
            {
                throw new ArgumentNullException(nameof(attachments));
            }

            _alreadySerializedDeliverMessage = new byte[0];

            Message = message;
            ContentType = contentType;
            Attachments = attachments;
        }

        internal DeliverMessageEnvelope(
            MessageInfo messageInfo, 
            byte[] deliverMessage, 
            string contentType) : this(messageInfo, deliverMessage, contentType, Enumerable.Empty<Attachment>()) { }

        internal DeliverMessageEnvelope(
            MessageInfo messageInfo, 
            byte[] deliverMessage, 
            string contentType, 
            IEnumerable<Attachment> attachments)
        {
            if (messageInfo == null)
            {
                throw new ArgumentNullException(nameof(messageInfo));
            }

            if (deliverMessage == null)
            {
                throw new ArgumentNullException(nameof(deliverMessage));
            }

            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            if (attachments == null)
            {
                throw new ArgumentNullException(nameof(attachments));
            }

            _alreadySerializedDeliverMessage = deliverMessage;

            Message = new DeliverMessage { MessageInfo = messageInfo };
            ContentType = contentType;
            Attachments = attachments;
        }

        public byte[] SerializeMessage()
        {
            if (_alreadySerializedDeliverMessage.Length == 0)
            {
                _alreadySerializedDeliverMessage = Encoding.UTF8.GetBytes(AS4XmlSerializer.ToString(Message));
            }

            return _alreadySerializedDeliverMessage;
        }
    }
}