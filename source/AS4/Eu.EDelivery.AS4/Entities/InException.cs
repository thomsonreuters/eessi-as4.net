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
        public ErrorAlias ErrorAlias { get; set; }

        [Column("ExceptionType")]
        public string ExceptionTypeString
        {
            get { return this.ErrorAlias.ToString(); }
            set { this.ErrorAlias = (ErrorAlias)Enum.Parse(typeof(ErrorAlias), value, true); }
        }
    }
}