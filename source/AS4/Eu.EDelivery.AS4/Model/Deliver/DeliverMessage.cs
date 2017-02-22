using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Common;

namespace Eu.EDelivery.AS4.Model.Deliver
{
    /// <summary>
    /// Describes all the fields that could be passed to the consuming business application when delivering messages. 
    /// </summary>
    public class DeliverMessage : IMessage
    {
        public MessageInfo MessageInfo { get; set; }
        public PartyInfo PartyInfo { get; set; }
        public CollaborationInfo CollaborationInfo { get; set; }
        public MessageProperty[] MessageProperties { get; set; }
        public Payload[] Payloads { get; set; }

        public DeliverMessage()
        {
            this.MessageInfo = new MessageInfo();
            this.CollaborationInfo = new CollaborationInfo();
            this.Payloads = new Payload[] { };
            this.PartyInfo = new PartyInfo();
        }
    }
}
