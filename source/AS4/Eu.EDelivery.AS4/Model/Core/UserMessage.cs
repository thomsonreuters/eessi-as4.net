using System.Collections.Generic;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Factories;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class UserMessage : MessageUnit
    {
        public string Mpc { get; set; }
        public Party Sender { get; set; }
        public Party Receiver { get; set; }

        public CollaborationInfo CollaborationInfo { get; set; }
        public IList<MessageProperty> MessageProperties { get; set; }
        public IList<PartInfo> PayloadInfo { get; set; }

        [XmlIgnore]
        public bool IsDuplicate { get; set; }

        [XmlIgnore]
        public bool IsTest { get; set; }

        public bool ShouldSerializePayloadInfo()
        {
            // When this method returns false, the XmlSerializer will not write an XmlElement for the PayloadInfo attribute.
            return PayloadInfo != null && PayloadInfo.Count > 0;
        }

        public UserMessage() : this(IdentifierFactory.Instance.Create())
        {
        }

        public UserMessage(string messageId) : base(messageId)
        {
            InitializeFields();
        }

        private void InitializeFields()
        {
            Sender = new Party(new PartyId { Id = Constants.Namespaces.EbmsDefaultFrom }) { Role = Constants.Namespaces.EbmsDefaultFrom };
            Receiver = new Party(new PartyId { Id = Constants.Namespaces.EbmsDefaultTo }) { Role = Constants.Namespaces.EbmsDefaultTo };
            CollaborationInfo = new CollaborationInfo();
            MessageProperties = new List<MessageProperty>();
            PayloadInfo = new List<PartInfo>();
        }


        public override string ToString()
        {
            return $"UserMessage [${this.MessageId}]";
        }
    }
}