using System;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    public class SendResult : IEquatable<SendResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendResult"/> class.
        /// </summary>
        /// <param name="status">The status.</param>
        private SendResult(SendStatus status)
        {
            Status = status;
        }

        /// <summary>
        /// Gets the status indicating whether the <see cref="SendResult"/> is successful or not.
        /// </summary>
        /// <value>The status.</value>
        public SendStatus Status { get; }

        /// <summary>
        /// Creates a successful representation of the <see cref="SendResult"/>.
        /// </summary>
        /// <returns></returns>
        public static SendResult Success { get; } = new SendResult(SendStatus.Success);

        /// <summary>
        /// Creates a failure representation of the <see cref="SendResult"/> with a flag indicating that the delivery can be retried.
        /// </summary>
        public static SendResult RetryableFail { get; } = new SendResult(SendStatus.RetryableFail);


        /// <summary>
        /// Creates a failure representation of the <see cref="SendResult"/> with a flag indicating that the delivery cannot be retried.
        /// </summary>
        public static SendResult FatalFail { get; } = new SendResult(SendStatus.Fail);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(SendResult other)
        {
            return other?.Status.Equals(Status) ?? false;
        }
    }
}