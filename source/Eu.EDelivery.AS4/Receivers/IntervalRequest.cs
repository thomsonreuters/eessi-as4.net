using System;

namespace Eu.EDelivery.AS4.Receivers
{
    /// <summary>
    /// Request implementation to use at each interval in the <see cref="ExponentialIntervalReceiver{T}"/>.
    /// </summary>
    public class IntervalRequest
    {
        private const double Factor = 1.75;

        private readonly TimeSpan _minInterval;
        private readonly TimeSpan _maxInterval;

        private int _runs;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntervalRequest"/> class.
        /// </summary>
        /// <param name="minInterval">The min Interval.</param>
        /// <param name="maxInterval">The max Interval.</param>
        protected IntervalRequest(TimeSpan minInterval, TimeSpan maxInterval)
        {
            _minInterval = minInterval;
            _maxInterval = maxInterval;
        }

        public TimeSpan CurrentInterval { get; private set; }

        /// <summary>
        /// Reset the <see cref="IntervalRequest"/> interval.
        /// </summary>
        public void ResetInterval()
        {
            _runs = 0;
            CurrentInterval = _minInterval;
        }

        /// <summary>
        /// Recalculate when the <see cref="IntervalRequest"/> must be resend.
        /// </summary>
        public void CalculateNewInterval()
        {
            if (CurrentInterval >= _maxInterval)
            {
                return;
            }

            var ticks = (long)(_minInterval.Ticks * Math.Pow(Factor, _runs));
            CurrentInterval = TimeSpan.FromTicks(ticks);

            if (CurrentInterval > _maxInterval)
            {
                CurrentInterval = _maxInterval;
            }

            _runs++;
        }
    }
}