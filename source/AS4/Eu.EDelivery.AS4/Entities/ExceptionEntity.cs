using System;
using System.ComponentModel.DataAnnotations;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionEntity"/> class.
        /// </summary>
        public ExceptionEntity()
        {
            SetOperation(default(Operation));
        }

        public void SetOperation(Operation operation)
        {
            Operation = operation.ToString();
        }

        [Column("Operation")]
        [MaxLength(50)]
        public string Operation { get; private set; }

        /// <summary>
        /// Update the <see cref="Entity" /> to lock it with a given <paramref name="value" />.
        /// </summary>
        /// <param name="value">Value indicating the <see cref="Entity" /> is locked.</param>
        public override void Lock(string value)
        {
            var updatedOperation = OperationUtils.Parse(value);
            SetOperation(updatedOperation);
        }
    }
}