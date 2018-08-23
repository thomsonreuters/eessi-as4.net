using System.Xml.Serialization;

namespace Eu.EDelivery.AS4.Xml
{
    public partial class Messaging
    {
        [XmlElement("UserMessage", typeof(UserMessage))]
        [XmlElement("SignalMessage", typeof(SignalMessage))]
        public object[] MessageUnits { get; set; }
    }
}
