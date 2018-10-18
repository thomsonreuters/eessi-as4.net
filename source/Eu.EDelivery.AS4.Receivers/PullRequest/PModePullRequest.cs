using System;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Receivers.PullRequest
{
    /// <summary>
    /// <see cref="IntervalRequest"/> implementation to request a <see cref="PullRequest"/> for a given <see cref="SendingProcessingMode"/>.
    /// </summary>
    public class PModePullRequest : IntervalRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PModePullRequest"/> class.
        /// </summary>
        /// <param name="pmode">The pmode.</param>
        /// <param name="minInterval">The min Interval.</param>
        /// <param name="maxInterval">The max Interval.</param>
        public PModePullRequest(SendingProcessingMode pmode, TimeSpan minInterval, TimeSpan maxInterval)
            : base(minInterval, maxInterval)
        {
            PMode = pmode;
        }

        public SendingProcessingMode PMode { get; }
    }
}