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

        public byte[] MessageBody { get; set; }

        public DateTimeOffset ModificationTime { get; set; }

        public DateTimeOffset InsertionTime { get; set; }

        [NotMapped]
        public Operation Operation { get; set; }

        public string OperationMethod { get; set; }

        [Column("Operation")]
        public string OperationString
        {
            get { return Operation.ToString(); }
            set { Operation = (Operation) Enum.Parse(typeof(Operation), value, true); }
        }

        /// <summary>
        /// Update the <see cref="Entity" /> to lock it with a given <paramref name="value" />.
        /// </summary>
        /// <param name="value">Value indicating the <see cref="Entity" /> is locked.</param>
        public override void Lock(string value)
        {
            var updatedOperation = (Operation) Enum.Parse(typeof(Operation), value);

            if (updatedOperation != Operation.NotApplicable)
            {
                Operation = updatedOperation;
            }
        }
    }
}