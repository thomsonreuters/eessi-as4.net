using Eu.EDelivery.AS4.Model.Deliver;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    /// <summary>
    /// Representation of the result after ther <see cref="DeliverMessage"/> has been send.
    /// </summary>
    public class DeliverResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeliverResult"/> class.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="anotherRetryIsNeeded">if set to <c>true</c> [another retry is needed].</param>
        public DeliverResult(DeliveryStatus status, bool anotherRetryIsNeeded)
        {
            Status = status;
            AnotherRetryIsNeeded = anotherRetryIsNeeded;
        }

        /// <summary>
        /// Gets the status indicating whether the <see cref="DeliverResult"/> is successful or not.
        /// </summary>
        /// <value>The status.</value>
        public DeliveryStatus Status { get; }

        /// <summary>
        /// Gets a value indicating whether [another retry is needed].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [another retry is needed]; otherwise, <c>false</c>.
        /// </value>
        public bool AnotherRetryIsNeeded { get; }

        /// <summary>
        /// Creates a successful representation of the <see cref="DeliverResult"/>.
        /// </summary>
        /// <returns></returns>
        public static DeliverResult Success => new DeliverResult(DeliveryStatus.Successful, anotherRetryIsNeeded: false);

        /// <summary>
        /// Creates as failure representation of the <see cref="DeliverResult"/>.
        /// </summary>
        /// <param name="anotherRetryIsNeeded">if set to <c>true</c> [another retry is needed].</param>
        /// <returns></returns>
        public static DeliverResult Failure(bool anotherRetryIsNeeded)
        {
            return new DeliverResult(DeliveryStatus.Failure, anotherRetryIsNeeded);
        }
    }

    public enum DeliveryStatus
    {
        Successful,
        Failure
    }
}