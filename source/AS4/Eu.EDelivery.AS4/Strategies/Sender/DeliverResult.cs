namespace Eu.EDelivery.AS4.Strategies.Sender
{
    public class DeliverResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeliverResult"/> class.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="needsAnotherRetry">if set to <c>true</c> [needs another retry].</param>
        public DeliverResult(DeliveryStatus status, bool needsAnotherRetry)
        {
            Status = status;
            NeedsAnotherRetry = needsAnotherRetry;
        }


        /// <summary>
        /// Gets the status indicating whether the <see cref="DeliverMessageResult"/> is successful or not.
        /// </summary>
        /// <value>The status.</value>
        public DeliveryStatus Status { get; }

        /// <summary>
        /// Gets a value indicating whether [another retry is needed].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [another retry is needed]; otherwise, <c>false</c>.
        /// </value>
        public bool NeedsAnotherRetry { get; }

        /// <summary>
        /// Reduces the two specified <see cref="DeliverResult"/>'s.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        public static DeliverResult Reduce(DeliverResult x, DeliverResult y)
        {
            return new DeliverResult(
                x.Status == DeliveryStatus.Successful 
                && y.Status == DeliveryStatus.Successful 
                    ? DeliveryStatus.Successful
                    : DeliveryStatus.Failure,
                x.NeedsAnotherRetry || y.NeedsAnotherRetry);
        }
    }
}