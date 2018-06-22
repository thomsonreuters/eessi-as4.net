using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Eu.EDelivery.AS4.Extensions;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// The reception awareness is responsible to keep track of the retry mechanism of then S-MSH
    /// </summary>
    public class ReceptionAwareness : Entity
    {
        [Required]
        public long RefToOutMessageId { get; private set; }

        [Required]
        [MaxLength(256)]
        public string RefToEbmsMessageId { get; private set; }

        public int CurrentRetryCount { get; set; }

        public int TotalRetryCount { get; set; }

        [MaxLength(12)]
        public string RetryInterval { get; set; }

        /// <summary>
        /// Contains the date/time when the message has last been sent.
        /// If the message hasn't been sent yet, this property returns null.
        /// </summary>
        public DateTimeOffset? LastSendTime { get; set; }

        [MaxLength(25)]
        public string Status
        {
            get;
            private set;
        }

        public void SetStatus(ReceptionStatus status)
        {
            this.Status = status.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceptionAwareness"/> class.
        /// </summary>
        private ReceptionAwareness()
        {
            SetStatus(default(ReceptionStatus));
        }

        public ReceptionAwareness(long refToOutMessageId, string refToEbmsMessageId) : this()
        {
            RefToOutMessageId = refToOutMessageId;
            RefToEbmsMessageId = refToEbmsMessageId;
        }

        public static ReceptionAwareness GetDetachedEntityForDatabaseUpdate(long receptionAwarenessId)
        {
            var ra = new ReceptionAwareness();
            ra.InitializeIdFromDatabase(receptionAwarenessId);

            return ra;
        }

        /// <summary>
        /// Update the <see cref="Entity"/> to lock it with a given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Value indicating the <see cref="Entity"/> is locked.</param>
        public override void Lock(string value)
        {
            var updatedStatus = value.ToEnum<ReceptionStatus>();

            SetStatus(updatedStatus);
        }
    }

    public enum ReceptionStatus
    {
        Pending,
        Busy,
        Completed
    }
}