using System;
using System.ComponentModel.DataAnnotations.Schema;
using Eu.EDelivery.AS4.Exceptions;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// AS4 Message Entity
    /// </summary>
    public abstract class MessageEntity : Entity
    {
        public string EbmsMessageId { get; set; }

        public string EbmsRefToMessageId { get; set; }

        public string ContentType { get; set; }

        public string PMode { get; set; }

        public byte[] MessageBody { get; set; }

        [NotMapped]
        public Operation Operation { get; set; }

        [Column("Operation")]
        public string OperationString
        {
            get { return Operation.ToString(); }
            set { Operation = (Operation) Enum.Parse(typeof(Operation), value, true); }
        }

        public DateTimeOffset InsertionTime { get; set; }

        public DateTimeOffset ModificationTime { get; set; }

        [NotMapped]
        public MessageExchangePattern MEP { get; set; }

        [NotMapped]
        public MessageType EbmsMessageType { get; set; }

        [NotMapped]
        public ExceptionType ExceptionType { get; set; }

        [Column("MEP")]
        public string MEPString
        {
            get { return MEP.ToString(); }
            set { MEP = (MessageExchangePattern) Enum.Parse(typeof(MessageExchangePattern), value, true); }
        }

        [Column("EbmsMessageType")]
        public string EbmsMessageTypeString
        {
            get { return EbmsMessageType.ToString(); }
            set { EbmsMessageType = (MessageType) Enum.Parse(typeof(MessageType), value, true); }
        }

        [Column("ExceptionType")]
        public string ExceptionTypeString
        {
            get { return ExceptionType.ToString(); }
            set { ExceptionType = (ExceptionType) Enum.Parse(typeof(ExceptionType), value, true); }
        }

        [Column("Status")]
        public abstract string StatusString { get; set; }

        /// <summary>
        /// Update the <see cref="Entity"/> to lock it with a given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Value indicating the <see cref="Entity"/> is locked.</param>
        public override void Lock(string value)
        {
            var updatedOperation = (Operation) Enum.Parse(typeof(Operation), value, ignoreCase: true);

            if (updatedOperation != Operation.NotApplicable)
            {
                Operation = updatedOperation;
            }
        }
    }
}