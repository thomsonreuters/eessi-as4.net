using System;
using Eu.EDelivery.AS4.Factories;

namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// Interface for the AS4 Messages (<see cref="UserMessage"/> and <see cref="SignalMessage"/>)
    /// </summary>
    public abstract class MessageUnit : IEquatable<MessageUnit>
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

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(MessageUnit other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(MessageId, other.MessageId)
                   && string.Equals(RefToMessageId, other.RefToMessageId);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is MessageUnit m && Equals(m);
        }

        /// <summary>
        /// Serves as the default hash function. 
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((MessageId != null ? MessageId.GetHashCode() : 0) * 397) ^ (RefToMessageId != null ? RefToMessageId.GetHashCode() : 0);
            }
        }

        /// <summary>Returns a value that indicates whether the values of two <see cref="T:Eu.EDelivery.AS4.Model.Core.MessageUnit" /> objects are equal.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(MessageUnit left, MessageUnit right)
        {
            return Equals(left, right);
        }

        /// <summary>Returns a value that indicates whether two <see cref="T:Eu.EDelivery.AS4.Model.Core.MessageUnit" /> objects have different values.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(MessageUnit left, MessageUnit right)
        {
            return !Equals(left, right);
        }
    }
}