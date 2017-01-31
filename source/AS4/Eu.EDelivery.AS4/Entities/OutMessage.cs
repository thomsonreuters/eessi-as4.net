using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    ///     Outcomming Message Data Entity Schema
    /// </summary>
    public class OutMessage : MessageEntity
    {
        [NotMapped]
        public OutStatus Status { get; set; }

        [Column("Status")]
        public string OutOutStatusString
        {
            get { return Status.ToString(); }
            set { Status = (OutStatus) Enum.Parse(typeof(OutStatus), value, true); }
        }
    }
}