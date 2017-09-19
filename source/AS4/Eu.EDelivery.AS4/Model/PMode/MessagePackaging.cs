using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Model.PMode
{
    public class MessagePackaging
    {
        public PartyInfo PartyInfo { get; set; }

        public CollaborationInfo CollaborationInfo { get; set; }        
        
        [XmlArray("MessageProperties")]
        [XmlArrayItem("MessageProperty")]
        public List<MessageProperty> MessageProperties { get; set; }
    }

    public class PartyInfo
    {
        public Party FromParty { get; set; }
        public Party ToParty { get; set; }

        /// <summary>
        /// Determine if the Party Properties are set
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return (FromParty == null || FromParty.IsEmpty()) &&
                   (ToParty == null || ToParty.IsEmpty());
        }        
    }
}