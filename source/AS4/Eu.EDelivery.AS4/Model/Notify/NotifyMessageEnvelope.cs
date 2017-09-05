using System;

namespace Eu.EDelivery.AS4.Model.Notify
{
    public class NotifyMessageEnvelope
    {
        public MessageInfo MessageInfo { get; }

        public Status StatusCode { get; }

        public string ContentType { get; private set; }

        public byte[] NotifyMessage { get; private set; }

        public Type EntityType { get; }

        public NotifyMessageEnvelope(MessageInfo messageInfo, Status statusCode, byte[] notifyMessage, string contentType, Type entityType)
        {
            MessageInfo = messageInfo;
            StatusCode = statusCode;
            NotifyMessage = notifyMessage;
            ContentType = contentType;
            EntityType = entityType;
        }
    }
}