using System;
using System.ComponentModel.DataAnnotations;

namespace Eu.EDelivery.AS4.Entities
{
    public class RetryReliability
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryReliability"/> class.
        /// </summary>
        public RetryReliability()
        {
            CurrentRetryCount = 0;
            MaxRetryCount = 0;
            RetryInterval = "0:00:00:00,0000000";
        }

        public long Id { get; set; }

        public long? RefToInMessageId { get; set; }

        public long? RefToOutMessageId { get; set; }

        public long? RefToInExceptionId { get; set; }

        public long? RefToOutExceptionId { get; set; }

        [MaxLength(12)]
        public string RetryType { get; private set; }

        public void SetRetryType(RetryType t)
        {
            RetryType = t.ToString();
        }

        public int CurrentRetryCount { get; set; }

        public int MaxRetryCount { get; set; }

        [MaxLength(50)]
        public string RetryInterval { get; private set; }

        public void SetRetryInterval(TimeSpan t)
        {
            RetryInterval = t.ToString("G");
        }

        [MaxLength(25)]
        public string Status { get; private set; }

        public void SetStatus(ReceptionStatus s)
        {
            Status = s.ToString();
        }

        public DateTimeOffset LastRetryTime { get; set; }
    }

    public enum RetryType
    {
        Delivery,
        Notification
    }
}
