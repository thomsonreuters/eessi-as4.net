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

        public virtual string GetActionValue()
        {
            return string.Empty;
        }
        
        /// <summary>
        /// RoutingInformation that is necessary for MultiHop messaging.
        /// </summary>
        public RoutingInputUserMessage MultiHopRouting { get; set; }
    }
}