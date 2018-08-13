using Eu.EDelivery.AS4.Model.Common;

namespace Eu.EDelivery.AS4.Model.Deliver
{
    public class DeliverMessageEnvelope
    {
        public MessageInfo MessageInfo { get; }

        public string ContentType { get; private set; }

        public byte[] DeliverMessage { get; set; }

        public DeliverMessageEnvelope(MessageInfo messageInfo, byte[] deliverMessage, string contentType)
        {
            this.MessageInfo = messageInfo;
            this.DeliverMessage = deliverMessage;
            this.ContentType = contentType;
        }
    }
}