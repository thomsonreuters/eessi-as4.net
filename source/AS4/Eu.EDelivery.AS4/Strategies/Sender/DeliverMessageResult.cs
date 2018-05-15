using Eu.EDelivery.AS4.Model.Deliver;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    /// <summary>
    /// Representation of the result after ther <see cref="DeliverMessage"/> has been send.
    /// </summary>
    public class DeliverMessageResult : DeliverResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeliverMessageResult"/> class.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="needsAnotherRetry">if set to <c>true</c> [another retry is needed].</param>
        private DeliverMessageResult(DeliveryStatus status, bool needsAnotherRetry) :
            base(status, needsAnotherRetry) { }

        /// <summary>
        /// Creates a successful representation of the <see cref="DeliverMessageResult"/>.
        /// </summary>
        /// <returns></returns>
        public static DeliverMessageResult Success => new DeliverMessageResult(DeliveryStatus.Successful, needsAnotherRetry: false);

        /// <summary>
        /// Creates as failure representation of the <see cref="DeliverMessageResult"/>.
        /// </summary>
        /// <param name="anotherRetryIsNeeded">if set to <c>true</c> [another retry is needed].</param>
        /// <returns></returns>
        public static DeliverMessageResult Failure(bool anotherRetryIsNeeded)
        {
            return new DeliverMessageResult(DeliveryStatus.Failure, anotherRetryIsNeeded);
        }
    }

    public enum DeliveryStatus
    {
        Successful,
        Failure
    }
}