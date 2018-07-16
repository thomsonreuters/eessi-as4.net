using System;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Xml;

namespace Eu.EDelivery.AS4.Model.Core
{
    public abstract class SignalMessage : MessageUnit
    {
        [XmlIgnore] public bool IsDuplicate { get; set; }

        protected SignalMessage() {}

        protected SignalMessage(string messageId) : base(messageId) {}
        protected SignalMessage(string messageId, string refToMessageId) : base(messageId, refToMessageId) { }

        protected SignalMessage(string messageId, string refToMessageId) : base(messageId, refToMessageId) { }

        protected SignalMessage(
            string messageId, 
            string refToMessageId, 
            DateTimeOffset timestamp) 
            : base(messageId, refToMessageId, timestamp) { }

        protected SignalMessage(
            string messageId, 
            string refToMessageId, 
            RoutingInputUserMessage routedUserMessage)
            : base(messageId, refToMessageId)
        {
            MultiHopRouting = routedUserMessage;
        } 

        /// <summary>
        /// Gets the multihop action value.
        /// </summary>
        public virtual string MultihopAction { get; } = String.Empty;

        /// <summary>
        /// RoutingInformation that is necessary for MultiHop messaging.
        /// </summary>
        public RoutingInputUserMessage MultiHopRouting { get; set; }
    }
}