using System;
using System.Linq;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    public class DeliverResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeliverResult"/> class.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="eligeableForRetry">if set to <c>true</c> [needs another retry].</param>
        public DeliverResult(DeliveryStatus status, bool eligeableForRetry)
        {
            Status = status;
            EligeableForRetry = eligeableForRetry;
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
        public bool EligeableForRetry { get; }

        /// <summary>
        /// Creates a successful representation of the <see cref="DeliverResult"/>.
        /// </summary>
        /// <returns></returns>
        public static DeliverResult Success { get; } = new DeliverResult(DeliveryStatus.Successful, eligeableForRetry: false);

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
                x.EligeableForRetry || y.EligeableForRetry);
        }

        /// <summary>
        /// Creates as failure representation of the <see cref="DeliverResult"/>.
        /// </summary>
        /// <param name="eligeableForRetry">if set to <c>true</c> [another retry is needed].</param>
        /// <returns></returns>
        public static DeliverResult Failure(bool eligeableForRetry)
        {
            return new DeliverResult(DeliveryStatus.Failure, eligeableForRetry);
        }
    }

    public enum DeliveryStatus
    {
        Successful,
        Failure
    }

    public static class ActionExtensions
    {
        public static Action<TResult> Select<T, TResult>(this Action<T> f, Func<TResult, T> g)
        {
            return x => f(g(x));
        }

        public static Func<TResult> Select<T, TResult>(this Func<T> f, Func<T, TResult> g)
        {
            return () => g(f());
        }

        public static Func<TResult> SelectMany<T, TResult>(this Func<T> f, Func<T, Func<TResult>> g)
        {
            return g(f());
        }

        public static Action<TResult> SelectM

        public static TResult Aggegrate<T, TResult>(this Func<T> f, TResult x, Func<TResult, Func<T>, TResult> g)
        {
            return g(x, f);
        }
    }

    public class ActionWorkSpace
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionWorkSpace"/> class.
        /// </summary>
        public ActionWorkSpace()
        {
            new Func<int>(() => 1)
             .Select(i => i + 1)
             .SelectMany<int, int>(i => () => i)
             .Aggegrate(0, (i, f) => f());
             

        }
    }
}