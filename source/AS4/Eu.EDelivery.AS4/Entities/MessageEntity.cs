using System;
using System.ComponentModel.DataAnnotations.Schema;
using Eu.EDelivery.AS4.Exceptions;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// AS4 Message Entity
    /// </summary>
    public class MessageEntity : Entity
    {
        public string EbmsMessageId { get; set; }
        public string EbmsRefToMessageId { get; set; }
        public string ContentType { get; set; }
        public string PMode { get; set; }

        public byte[] MessageBody { get; set; }

        [NotMapped] public Operation Operation { get; set; }
        public string OperationMethod { get; set; }

        [Column("Operation")]
        public string OperationString
        {
            get { return this.Operation.ToString(); }
            set { this.Operation = (Operation)Enum.Parse(typeof(Operation), value, true); }
        }

        public DateTimeOffset InsertionTime { get; set; }
        public DateTimeOffset ModificationTime { get; set; }
        [NotMapped] public MessageExchangePattern MEP { get; set; }
        [NotMapped] public MessageType EbmsMessageType { get; set; }
        [NotMapped] public ExceptionType ExceptionType { get; set; }

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
