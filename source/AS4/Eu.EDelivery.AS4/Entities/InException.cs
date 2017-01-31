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
        public ExceptionType ExceptionType { get; set; }

        [Column("ExceptionType")]
        public string ExceptionTypeString
        {
            get { return this.ExceptionType.ToString(); }
            set { this.ExceptionType = (ExceptionType)Enum.Parse(typeof(ExceptionType), value, true); }
        }
    }
}