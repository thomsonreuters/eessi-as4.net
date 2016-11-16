using System;
using System.ComponentModel.DataAnnotations.Schema;
using Eu.EDelivery.AS4.Exceptions;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// Incoming Message Exception Data Entity Schema
    /// </summary>
    public class InException : ExceptionEntity
    {
        [NotMapped]
        public Operation Operation { get; set; }
        [NotMapped]
        public ExceptionType ExceptionType { get; set; }
        public string OperationMethod { get; set; }

        [Column("Operation")]
        public string OperationString
        {
            get { return this.Operation.ToString(); }
            set { this.Operation = (Operation)Enum.Parse(typeof(Operation), value, true); }
        }

        [Column("ExceptionType")]
        public string ExceptionTypeString
        {
            get { return this.ExceptionType.ToString(); }
            set { this.ExceptionType = (ExceptionType)Enum.Parse(typeof(ExceptionType), value, true); }
        }
    }
}