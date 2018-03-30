using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Model.Core;
using Newtonsoft.Json;

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

        #region Serialization Control properties

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool FromPartySpecified
        {
            get
            {
                bool hasRole = !string.IsNullOrEmpty(FromParty?.Role);
                bool hasPartyId = FromParty?.PartyIds?.All(p => !string.IsNullOrEmpty(p.Id)) ?? false;

                return hasRole && hasPartyId;
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool ToPartySpecified
        {
            get
            {
                bool hasRole = !string.IsNullOrEmpty(ToParty?.Role);
                bool hasPartyId = ToParty?.PartyIds?.All(p => !string.IsNullOrEmpty(p.Id)) ?? false;

                return hasRole && hasPartyId;
            }
        }

        #endregion
    }
}