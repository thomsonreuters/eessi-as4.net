using System.Collections.Generic;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Model.Deliver
{
    public class DeliverMessageEnvelope
    {
        public MessageInfo MessageInfo { get; }

        public string ContentType { get; private set; }

        public byte[] DeliverMessage { get; set; }

        public IEnumerable<Attachment> Payloads { get; }

        public DeliverMessageEnvelope(MessageInfo messageInfo, byte[] deliverMessage, string contentType)
        {
            this.MessageInfo = messageInfo;
            this.DeliverMessage = deliverMessage;
            this.ContentType = contentType;
        }

        public DeliverMessageEnvelope(MessageInfo messageInfo, byte[] deliverMessage, string contentType, IEnumerable<Attachment> payloads)
        {
            MessageInfo = messageInfo;
            ContentType = contentType;
            DeliverMessage = deliverMessage;
            Payloads = payloads;
        }
    }
}