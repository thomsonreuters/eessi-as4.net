using System;
using System.Xml.Serialization;

namespace Eu.EDelivery.AS4.Xml
{
    /// <summary>
    /// Adding Concrete information to the Receipt
    /// </summary>
    [XmlRoot(Namespace = "http://docs.oasis-open.org/ebxml-bp/ebbp-signals-2.0", IsNullable = false)]    
    public partial class Receipt
    {
        public UserMessage UserMessage { get; set; }
        public NonRepudiationInformation NonRepudiationInformation { get; set; }
    }
}
