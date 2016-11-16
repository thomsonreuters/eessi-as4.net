using System;
using System.ComponentModel.DataAnnotations.Schema;
using Eu.EDelivery.AS4.Exceptions;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// Outcomming Message Data Entity Schema
    /// </summary>
    public class OutMessage : MessageEntity
    {
        [NotMapped] public MessageExchangePattern MEP { get; set; }
        [NotMapped] public MessageType EbmsMessageType { get; set; }
        [NotMapped] public OutStatus Status { get; set; }
        [NotMapped] public ExceptionType ExceptionType { get; set; }

        [Column("Status")]
        public string OutOutStatusString
        {
            get { return this.Status.ToString(); }
            set { this.Status = (OutStatus) Enum.Parse(typeof(OutStatus), value, true); }
        }

        [Column("MEP")]
        public string MEPString
        {
            get { return this.MEP.ToString(); }
            set { this.MEP = (MessageExchangePattern) Enum.Parse(typeof(MessageExchangePattern), value, true); }
        }

        [Column("EbmsMessageType")]
        public string EbmsMessageTypeString
        {
            get { return this.EbmsMessageType.ToString(); }
            set { this.EbmsMessageType = (MessageType)Enum.Parse(typeof(MessageType), value, true); }
        }

        [Column("ExceptionType")]
        public string ExceptionTypeString
        {
            get { return this.ExceptionType.ToString(); }
            set { this.ExceptionType = (ExceptionType)Enum.Parse(typeof(ExceptionType), value, true); }
        }
    }
}