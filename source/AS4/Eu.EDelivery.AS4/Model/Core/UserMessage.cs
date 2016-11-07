using System.Collections.Generic;
using System.Xml.Serialization;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class UserMessage : MessageUnit
    {
        public string Mpc { get; set; }
        public Party Sender { get; set; }
        public Party Receiver { get; set; }

        public CollaborationInfo CollaborationInfo { get; set; }
        public IList<MessageProperty> MessageProperties { get; set; }
        public List<PartInfo> PayloadInfo { get; set; }

        [XmlIgnore] public bool IsDuplicate { get; set; }
        [XmlIgnore] public bool IsTest { get; set; }

        public UserMessage()
        {
            InitializeFields();
        }

        public UserMessage(string messageId) : base(messageId)
        {
            InitializeFields();
        }

        private void InitializeFields()
        {
            const string fromNamespace = "http://docs.oasis-open.org/ebxml-msg/ebMS/v3.0/ns/core/200704/defaultFrom";
            const string toNamespace = "http://docs.oasis-open.org/ebxml-msg/ebMS/v3.0/ns/core/200704/defaultTo";

            this.Sender = new Party(new PartyId {Id = fromNamespace});
            this.Receiver = new Party(new PartyId {Id = toNamespace});
            this.CollaborationInfo = new CollaborationInfo();
        }


        public override string ToString()
        {
            return $"UserMessage [${this.MessageId}]";
        }
    }
}