using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// Entity for Exceptions
    /// </summary>
    public class ExceptionEntity : Entity
    {
        public string EbmsRefToMessageId { get; set; }
        public string Exception { get; set; }
        public string PMode { get; set; }

        public DateTimeOffset ModificationTime { get; set; }
        public DateTimeOffset InsertionTime { get; set; }
        [NotMapped] public Operation Operation { get; set; }
        public string OperationMethod { get; set; }

        [Column("Operation")]
        public string OperationString
        {
            get { return this.Operation.ToString(); }
            set { this.Operation = (Operation) Enum.Parse(typeof(Operation), value, true); }
        }
    }
}
