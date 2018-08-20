using System.Xml.Serialization;

namespace Eu.EDelivery.AS4.Xml
{
    public partial class Property
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlIgnore]
        public bool TypeSpecified
        {
            get { return Type != null; }
            set
            {
                if (value == false)
                {
                    Type = null;
                }
            }
        }
    }
}
