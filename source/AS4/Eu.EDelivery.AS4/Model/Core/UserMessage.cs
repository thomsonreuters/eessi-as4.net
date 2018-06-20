using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Factories;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class UserMessage : MessageUnit
    {
        private readonly List<PartInfo> _partInfos;

        public string Mpc { get; set; }
        public Party Sender { get; set; }
        public Party Receiver { get; set; }

        public CollaborationInfo CollaborationInfo { get; set; }
        public IList<MessageProperty> MessageProperties { get; set; }
        public IEnumerable<PartInfo> PayloadInfo => _partInfos.AsReadOnly();

        // TODO: this should happen together with the adding of Attachments
        // TODO: only unique PartInfo's are allowed
        public void AddPartInfo(PartInfo p)
        {
            _partInfos.Add(p);
        }

        [XmlIgnore]
        public bool IsDuplicate { get; set; }

        [XmlIgnore]
        public bool IsTest { get; set; }

        public bool ShouldSerializePayloadInfo()
        {
            // When this method returns false, the XmlSerializer will not write an XmlElement for the PayloadInfo attribute.
            return _partInfos.Any();
        }

        public UserMessage() : this(IdentifierFactory.Instance.Create())
        {

        }

        public UserMessage(string messageId) : base(messageId)
        {
            Sender = new Party(Constants.Namespaces.EbmsDefaultFrom, new PartyId(Constants.Namespaces.EbmsDefaultFrom));
            Receiver = new Party(Constants.Namespaces.EbmsDefaultTo, new PartyId(Constants.Namespaces.EbmsDefaultTo));
            CollaborationInfo = new CollaborationInfo();
            MessageProperties = new List<MessageProperty>();

            _partInfos = new List<PartInfo>();
        }


        public override string ToString()
        {
            return $"UserMessage [${this.MessageId}]";
        }
    }
}