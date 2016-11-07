using System;
using System.ComponentModel.DataAnnotations.Schema;

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
    }
}
