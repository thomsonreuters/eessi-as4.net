using System;
using Eu.EDelivery.AS4.Factories;

namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// Interface for the AS4 Messages (<see cref="UserMessage"/> and <see cref="SignalMessage"/>)
    /// </summary>
    public abstract class MessageUnit
    {
        public DateTimeOffset Timestamp { get; set; }

        public string MessageId { get; set; }

        public string RefToMessageId { get; set; }
       
        protected MessageUnit() : this(IdentifierFactory.Instance.Create()) { }

        protected MessageUnit(string messageid)
        {
            if (messageid == null)
            {
                throw new ArgumentNullException(nameof(messageid));
            }

            Timestamp = DateTimeOffset.Now;
            MessageId = messageid;
        }

        protected MessageUnit(string messageId, string refToMessageId)
        {
            if (messageId == null)
            {
                throw new ArgumentNullException(nameof(messageId));
            }

            if (refToMessageId == null)
            {
                throw new ArgumentNullException(nameof(refToMessageId));
            }

            Timestamp = DateTimeOffset.Now;
            MessageId = messageId;
            RefToMessageId = refToMessageId;
        }

        protected MessageUnit(string messageId, string refToMessageId, DateTimeOffset timestamp)
        {
            if (messageId == null)
            {
                throw new ArgumentNullException(nameof(messageId));
            }

            if (refToMessageId == null)
            {
                throw new ArgumentNullException(nameof(refToMessageId));
            }

            MessageId = messageId;
            RefToMessageId = refToMessageId;
            Timestamp = timestamp;
        }
    }
}