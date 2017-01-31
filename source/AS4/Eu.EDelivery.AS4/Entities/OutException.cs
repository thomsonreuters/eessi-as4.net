using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// Outcoming Message Exception Data Entity Schema
    /// </summary>
    public class OutException : ExceptionEntity
    {   
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