using System;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Xml;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class SignalMessage : MessageUnit
    {
        [XmlIgnore] public bool IsDuplicated { get; set; }

        public SignalMessage() {}
        public SignalMessage(string messageId) : base(messageId) {}

        public virtual string GetActionValue()
        {
            return string.Empty;
        }
    }
}