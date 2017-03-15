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

        public string Attachments { get; set; }

        //[Column("Status")]
        //public string InStatusString
        //{
        //    get { return Status.ToString(); }
        //    set { Status = (InStatus) Enum.Parse(typeof(InStatus), value, true); }
        //}
        public override string StatusString
        {
            get => Status.ToString();
            set => Status = (InStatus)Enum.Parse(typeof(InStatus), value, true);
        }
    }
}