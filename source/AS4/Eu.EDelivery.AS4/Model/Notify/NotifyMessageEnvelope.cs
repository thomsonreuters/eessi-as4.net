namespace Eu.EDelivery.AS4.Model.Notify
{
    public class NotifyMessageEnvelope
    {
        public MessageInfo MessageInfo { get; }

        public Status StatusCode { get; }
       
        public string ContentType { get; private set; }

        public byte[] NotifyMessage { get; private set; }

        public NotifyMessageEnvelope(MessageInfo messageInfo, Status statusCode, byte[] notifyMessage, string contentType )
        {
            this.MessageInfo = messageInfo;
            this.StatusCode = statusCode;
            this.NotifyMessage = notifyMessage;
            this.ContentType = contentType;
        }
    }
}