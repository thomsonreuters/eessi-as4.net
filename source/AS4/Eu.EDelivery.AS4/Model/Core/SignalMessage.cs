using System.Xml.Serialization;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class SignalMessage : MessageUnit
    {
        [XmlIgnore] public bool IsDuplicated { get; set; }

        public SignalMessage() {}
        public SignalMessage(string messageId) : base(messageId) {}
    }
}