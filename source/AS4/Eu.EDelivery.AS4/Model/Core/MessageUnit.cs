using System;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Utilities;

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

        protected MessageUnit()
        {
            this.Timestamp = DateTimeOffset.UtcNow;
            this.MessageId = IdentifierFactory.Instance.Create();
        }

        protected MessageUnit(string messageid)
        {
            this.Timestamp = DateTimeOffset.UtcNow;
            this.MessageId = messageid;
        }
    }
}