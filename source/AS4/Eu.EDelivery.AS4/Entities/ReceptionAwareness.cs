using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// The reception awareness is responsible to keep track of the retry mechanism of then S-MSH
    /// </summary>
    public class ReceptionAwareness : Entity
    {
        public string InternalMessageId { get; set; }

        public int CurrentRetryCount { get; set; }

        public int TotalRetryCount { get; set; }

        public string RetryInterval { get; set; }

        public DateTimeOffset LastSendTime { get; set; }

        [NotMapped]
        public ReceptionStatus Status { get; set; }

        [Column("Status")]
        public string StatusString
        {
            get { return Status.ToString(); }
            set { Status = (ReceptionStatus) Enum.Parse(typeof(ReceptionStatus), value, true); }
        }

        /// <summary>
        /// Update the <see cref="Entity"/> to lock it with a given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Value indicating the <see cref="Entity"/> is locked.</param>
        public override void Lock(string value)
        {            
            StatusString = value;
        }
    }

    public enum ReceptionStatus
    {
        Pending,
        Busy,
        Completed
    }
}