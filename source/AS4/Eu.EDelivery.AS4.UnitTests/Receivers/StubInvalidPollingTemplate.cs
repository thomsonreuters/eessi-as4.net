using System;
using System.Collections.Generic;
using System.Threading;
using Eu.EDelivery.AS4.Receivers;
using NLog;
using Function =
    System.Func<string, System.Threading.CancellationToken,
        System.Threading.Tasks.Task<Eu.EDelivery.AS4.Model.Internal.InternalMessage>>;

namespace Eu.EDelivery.AS4.UnitTests.Receivers
{
    /// <summary>
    /// Stub for the <see cref="PollingTemplate{TIn,TOut}" />
    /// </summary>
    public class StubInvalidPollingTemplate : PollingTemplate<string, string>
    {
        protected override ILogger Logger { get; } = LogManager.GetCurrentClassLogger();
        protected override TimeSpan PollingInterval { get; } = TimeSpan.FromSeconds(0);

        public bool IsMessageReceived { get; set; }
        public bool IsFailed { get; set; }

        /// <summary>
        /// Create new Stub Polling Template with Stubbed protected Methods
        /// </summary>
        /// <param name="messageReceived"></param>
        /// <param name="cancellationToken"></param>
        public void Start(Function messageReceived, CancellationToken cancellationToken)
        {
            StartPolling(messageReceived, cancellationToken);
        }

        protected override IEnumerable<string> GetMessagesToPoll(CancellationToken cancellationToken)
        {
            return new[] {"Message"};
        }

        protected override void HandleMessageException(string message, Exception exception)
        {
            this.IsFailed = true;
        }

        protected override void MessageReceived(
            string entity,
            Function messageCallback,
            CancellationToken cancellationToken)
        {
            this.IsMessageReceived = true;

            messageCallback(entity, cancellationToken);
        }

        protected override void ReleasePendingItems()
        {            
        }
    }
}