using System;
using System.ComponentModel.DataAnnotations;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// Persistence model to store the retry information during the Delivery, Notification 
    /// about other entity records (In/Out Message/Exception).
    /// </summary>
    public class RetryReliability
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryReliability"/> class.
        /// </summary>
        public RetryReliability()
        {
            CurrentRetryCount = 0;
            RetryInterval = "0:00:00:00";
            Status = ReceptionStatus.Pending.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryReliability"/> class.
        /// </summary>
        /// <param name="referencedEntity">Referenced entity to </param>
        /// <param name="maxRetryCount"></param>
        /// <param name="retryInterval"></param>
        /// <param name="type"></param>
        public RetryReliability(
            Entity referencedEntity,
            int maxRetryCount,
            TimeSpan retryInterval,
            RetryType type) : this()
        {
            if (referencedEntity is InMessage im)
            {
                RefToInMessageId = im.Id;
            }
            else if (referencedEntity is OutMessage om)
            {
                RefToOutMessageId = om.Id;
            }
            else if (referencedEntity is InException ie)
            {
                RefToInExceptionId = ie.Id;
            }
            else if (referencedEntity is OutException oe)
            {
                RefToOutExceptionId = oe.Id;
            }
            else
            {
                throw new ArgumentException(
                    $@"Only In/Out Message/Exception types are supported to reference a {nameof(RetryReliability)}",
                    paramName: nameof(referencedEntity));
            }

            MaxRetryCount = maxRetryCount;
            RetryInterval = retryInterval.ToString("G");
            RetryType = type.ToString();
        }

        public long Id { get; set; }

        public long? RefToInMessageId { get; private set; }

        public long? RefToOutMessageId { get; private set; }

        public long? RefToInExceptionId { get; private set; }

        public long? RefToOutExceptionId { get; private set; }

        [MaxLength(12)]
        public string RetryType { get; private set; }

        public int CurrentRetryCount { get; set; }

        public int MaxRetryCount { get; private set; }

        [MaxLength(50)]
        public string RetryInterval { get; private set; }

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
