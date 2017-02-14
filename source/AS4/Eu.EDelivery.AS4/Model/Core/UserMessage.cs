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
        }

        private void InitializeFields()
        {
            this.Sender = new Party(new PartyId {Id = Constants.Namespaces.EbmsDefaultFrom});
            this.Receiver = new Party(new PartyId {Id = Constants.Namespaces.EbmsDefaultTo});
            this.CollaborationInfo = new CollaborationInfo();
            this.MessageProperties = new List<MessageProperty>();
        }


        public override string ToString()
        {
            return $"UserMessage [${this.MessageId}]";
        }
    }
}