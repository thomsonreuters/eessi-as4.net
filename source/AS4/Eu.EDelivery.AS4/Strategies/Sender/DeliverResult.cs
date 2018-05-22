using System;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    public class DeliverResult : IEquatable<DeliverResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeliverResult"/> class.
        /// </summary>
        /// <param name="status">The status.</param>
        private DeliverResult(DeliveryStatus status)
        {
            Status = status;
        }

        /// <summary>
        /// Gets the status indicating whether the <see cref="DeliverResult"/> is successful or not.
        /// </summary>
        /// <value>The status.</value>
        public DeliveryStatus Status { get; }

        /// <summary>
        /// Creates a successful representation of the <see cref="DeliverResult"/>.
        /// </summary>
        /// <returns></returns>
        public static DeliverResult Success { get; } = new DeliverResult(DeliveryStatus.Success);

        /// <summary>
        /// Creates a failure representation of the <see cref="DeliverResult"/> with a flag indicating that the delivery can be retried.
        /// </summary>
        public static DeliverResult RetryableFail { get; } = new DeliverResult(DeliveryStatus.RetryableFail);


        /// <summary>
        /// Creates a failure representation of the <see cref="DeliverResult"/> with a flag indicating that the delivery cannot be retried.
        /// </summary>
        public static DeliverResult FatalFail { get; } = new DeliverResult(DeliveryStatus.Fail);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(DeliverResult other)
        {
            return other?.Status.Equals(Status) ?? false;
        }
    }
}