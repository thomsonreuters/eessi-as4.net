using System;
using System.ComponentModel.DataAnnotations;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// Persistence model to store the retry information during the Delivery, Notification 
    /// about other entity records (In/Out Message/Exception).
    /// </summary>
    public class RetryReliability : Entity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryReliability"/> class.
        /// </summary>
        public RetryReliability()
        {
            CurrentRetryCount = 0;
            RetryInterval = TimeSpan.Zero;
            Status = RetryStatus.Idle;
            LastRetryTime = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryReliability"/> class.
        /// </summary>
        /// <param name="maxRetryCount">The maximum retry count</param>
        /// <param name="retryInterval">The interval in which the retry should happen</param>
        /// <param name="type">The type of the retry</param>
        private RetryReliability(
            int maxRetryCount,
            TimeSpan retryInterval,
            RetryType type) : this()
        {
            MaxRetryCount = maxRetryCount;
            RetryInterval = retryInterval;
            RetryType = type;
        }

        public long? RefToInMessageId { get; private set; }

        public long? RefToOutMessageId { get; private set; }

        public long? RefToInExceptionId { get; private set; }

        public long? RefToOutExceptionId { get; private set; }

        [MaxLength(12)]
        public RetryType RetryType { get; }

        public int CurrentRetryCount { get; set; }

        public int MaxRetryCount { get; }

        [MaxLength(50)]
        public TimeSpan RetryInterval { get; }

        [MaxLength(25)]
        public RetryStatus Status { get; set; }

        [Obsolete]
        public void SetStatus(RetryStatus s)
        {
            Status = s;
        }

        public DateTimeOffset? LastRetryTime { get; set; }

        /// <summary>
        /// Creates a <see cref="RetryReliability"/> instance referencing a <see cref="InMessage"/>.
        /// </summary>
        /// <param name="refToInMessageId">Reference to the <see cref="InMessage"/> entity</param>
        /// <param name="maxRetryCount">The maximum retry count</param>
        /// <param name="retryInterval">The interval in which the retry should happen</param>
        /// <param name="type">The type of the retry</param>
        /// <returns></returns>
        public static RetryReliability CreateForInMessage(
            long refToInMessageId,
            int maxRetryCount,
            TimeSpan retryInterval,
            RetryType type)
        {
            return new RetryReliability(maxRetryCount, retryInterval, type)
            {
                RefToInMessageId = refToInMessageId
            };
        }

        /// <summary>
        /// Creates a <see cref="RetryReliability"/> instance referencing a <see cref="OutMessage"/>.
        /// </summary>
        /// <param name="refToOutMessageId">Reference to the <see cref="OutMessage"/> entity</param>
        /// <param name="maxRetryCount">The maximum retry count</param>
        /// <param name="retryInterval">The interval in which the retry should happen</param>
        /// <param name="type">The type of the retry</param>
        /// <returns></returns>
        public static RetryReliability CreateForOutMessage(
            long refToOutMessageId,
            int maxRetryCount,
            TimeSpan retryInterval,
            RetryType type)
        {
            return new RetryReliability(maxRetryCount, retryInterval, type)
            {
                RefToOutMessageId = refToOutMessageId
            };
        }

        /// <summary>
        /// Creates a <see cref="RetryReliability"/> instance referencing a <see cref="InException"/>.
        /// </summary>
        /// <param name="refToInExceptionId">Reference to the <see cref="InException"/> entity</param>
        /// <param name="maxRetryCount">The maximum retry count</param>
        /// <param name="retryInterval">The interval in which the retry should happen</param>
        /// <param name="type">The type of the retry</param>
        /// <returns></returns>
        public static RetryReliability CreateForInException(
            long refToInExceptionId,
            int maxRetryCount,
            TimeSpan retryInterval,
            RetryType type)
        {
            return new RetryReliability(maxRetryCount, retryInterval, type)
            {
                RefToInExceptionId = refToInExceptionId
            };
        }

        /// <summary>
        /// Creates a <see cref="RetryReliability"/> instance referencing a <see cref="OutException"/>.
        /// </summary>
        /// <param name="refToOutExceptionId">Reference to the <see cref="OutException"/> entity</param>
        /// <param name="maxRetryCount">The maximum retry count</param>
        /// <param name="retryInterval">The interval in which the retry should happen</param>
        /// <param name="type">The type of the retry</param>
        /// <returns></returns>
        public static RetryReliability CreateForOutException(
            long refToOutExceptionId,
            int maxRetryCount,
            TimeSpan retryInterval,
            RetryType type)
        {
            return new RetryReliability(maxRetryCount, retryInterval, type)
            {
                RefToOutExceptionId = refToOutExceptionId
            };
        }
    }

    public enum RetryType
    {
        Delivery,
        Notification
    }

    public enum RetryStatus
    {
        Idle,
        Pending,
        Busy,
        Completed
    }
}
