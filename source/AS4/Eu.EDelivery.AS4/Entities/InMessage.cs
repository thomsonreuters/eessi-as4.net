using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    ///     Incoming Message Data Entity Schema
    /// </summary>
    public class InMessage : MessageEntity
    {
        [NotMapped]
        public InStatus Status { get; set; }
    
        public override string StatusString
        {
            get
            {
                return Status.ToString();
            }
            set
            {
                Status = (InStatus)Enum.Parse(typeof(InStatus), value, true);
            }
        }
    }
}