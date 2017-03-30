using System.Xml.Serialization;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    [XmlType(Namespace = "eu:edelivery:as4:pmode")]
    [XmlRoot("PMode", Namespace = "eu:edelivery:as4:pmode", IsNullable = false)]
    public class Pmode
    {
        public string Id { get; set; }
    }
}