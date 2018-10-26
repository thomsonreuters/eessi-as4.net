using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
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

        #region Serialization Control properties

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool FromPartySpecified
        {
            get
            {
                bool hasRole = !String.IsNullOrEmpty(FromParty?.Role);
                bool hasPartyId = FromParty?.PartyIds?.All(p => !String.IsNullOrEmpty(p.Id)) ?? false;

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
                bool hasRole = !String.IsNullOrEmpty(ToParty?.Role);
                bool hasPartyId = ToParty?.PartyIds?.All(p => !String.IsNullOrEmpty(p.Id)) ?? false;

                return hasRole && hasPartyId;
            }
        }

        #endregion
    }
}