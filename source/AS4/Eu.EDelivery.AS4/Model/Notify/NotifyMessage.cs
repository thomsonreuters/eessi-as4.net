using System.Xml;
using System.Xml.Serialization;

namespace Eu.EDelivery.AS4.Model.Notify
{
    /// <summary>
    /// Describes all the fields that could be passed to the business application when delivering messages.
    /// </summary>
    public class NotifyMessage
    {
        public Notify.MessageInfo MessageInfo { get; set; }
        public StatusInfo StatusInfo { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyMessage"/> class
        /// </summary>
        public NotifyMessage()
        {
            this.MessageInfo = new MessageInfo();
            this.StatusInfo = new StatusInfo();
        }
    }

    public class MessageInfo
    {
        public string MessageId { get; set; }
        public string RefToMessageId { get; set; }
    }

    public class StatusInfo
    {
        public Status Status { get; set; }

        [XmlAnyElement]
        public XmlElement[] Any { get; set; }
    }

    public enum Status
    {
        Error,
        Delivered,
        Exception
    }
}