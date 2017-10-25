using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// The reception awareness is responsible to keep track of the retry mechanism of then S-MSH
    /// </summary>
    public class ReceptionAwareness : Entity
    {
        [MaxLength(256)]
        public string InternalMessageId { get; set; }

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
        public ReceptionAwareness()
        {
            SetStatus(default(ReceptionStatus));
        }

        /// <summary>
        /// Update the <see cref="Entity"/> to lock it with a given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Value indicating the <see cref="Entity"/> is locked.</param>
        public override void Lock(string value)
        {
            // TODO: use enum-utils method.
            var updatedStatus = ReceptionStatusUtils.Parse(value);

            SetStatus(updatedStatus);
        }
    }

    public enum ReceptionStatus
    {
        Pending,
        Busy,
        Completed
    }

    public static class ReceptionStatusUtils
    {
        public static ReceptionStatus Parse(string value)
        {
            return (ReceptionStatus)Enum.Parse(typeof(ReceptionStatus), value, true);
        }
    }
}