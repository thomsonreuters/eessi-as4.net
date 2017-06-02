using System.Xml.Serialization;

namespace Eu.EDelivery.AS4.Xml
{
    /// <summary>
    /// Adding Concrete information to the Receipt
    /// </summary>
    [XmlRoot(Namespace = Constants.Namespaces.EbmsXmlSignals, IsNullable = false)]
    public partial class Receipt
    {
        [XmlElement(Namespace = Constants.Namespaces.EbmsXmlSignals)]
        public UserMessage UserMessage { get; set; }

        [XmlElement(Namespace = Constants.Namespaces.EbmsXmlSignals)]
        public NonRepudiationInformation NonRepudiationInformation { get; set; }

    }
}
