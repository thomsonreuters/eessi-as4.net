using System;
using System.Xml;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Receivers.Pull
{
    public class PModeRequest
    {
        private const double Factor = 1.75;

        private readonly TimeSpan _minInterval;
        private readonly TimeSpan _maxInterval;

        private int _runs;

        public TimeSpan CurrentInterval { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PModeRequest"/> class.
        /// </summary>
        /// <param name="pmode">The pmode.</param>
        /// <param name="minInterval">The min Interval.</param>
        /// <param name="maxInterval">The max Interval.</param>
        public PModeRequest(SendingProcessingMode pmode, XmlAttribute minInterval, XmlAttribute maxInterval)
        {
            PMode = pmode;
            _minInterval = TimeSpan.Parse(minInterval.Value);
            _maxInterval = TimeSpan.Parse(maxInterval.Value);
        }

        public SendingProcessingMode PMode { get; }

        /// <summary>
        /// Reset the PMode Request.
        /// </summary>
        public void ResetInterval()
        {
            _runs = 0;
        }

        /// <summary>
        /// Recalculate when the Request must be resend.
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