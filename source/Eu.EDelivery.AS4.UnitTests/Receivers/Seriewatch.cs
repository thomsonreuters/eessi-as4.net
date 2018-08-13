using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Eu.EDelivery.AS4.UnitTests.Receivers
{
    /// <summary>
    /// Watch to track series during a running time
    /// </summary>
    public class Seriewatch
    {
        private readonly Stopwatch _stopwatch;
        private readonly IList<long> _series;

        private int _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="Seriewatch"/> class.
        /// </summary>
        public Seriewatch()
        {
            _count = -1;
            _stopwatch = new Stopwatch();
            _series = new List<long>();
        }

        /// <summary>
        /// Get the Serie for a given <paramref name="serieCount"/>.
        /// </summary>
        /// <param name="serieCount">Which serie to return.</param>
        /// <returns></returns>
        public long GetSerie(int serieCount)
        {
            return _series.ElementAtOrDefault(serieCount);
        }

        /// <summary>
        /// Track a next serie in the <see cref="Seriewatch"/>.
        /// </summary>
        /// <param name="maxSerieCount">Amount of series to track.</param>
        /// <returns></returns>
        public bool TrackSerie(int maxSerieCount)
        {
            if (++_count == 0)
            {
                _stopwatch.Start();
            }
            else
            {
                _series.Add(_stopwatch.ElapsedMilliseconds);
            }

            return _count >= maxSerieCount;
        }
    }
}