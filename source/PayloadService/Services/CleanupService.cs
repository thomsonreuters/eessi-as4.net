using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using Eu.EDelivery.AS4.PayloadService.Persistance;
using NLog;

namespace Eu.EDelivery.AS4.PayloadService.Services
{
    /// <summary>
    /// Service to run periodically to cleaning up the retired persisted payloads.
    /// </summary>
    public class CleanUpService
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IPayloadPersister _payloadPersister;
        private readonly TimeSpan _retentionPeriod;
        private readonly CancellationTokenSource __cancellation;

        /// <summary>
        /// Initializes a new instance of the <see cref="CleanUpService" /> class.
        /// </summary>
        /// <param name="payloadPersister">The payload persister.</param>
        /// <param name="retentionPeriod">The retention period.</param>
        public CleanUpService(IPayloadPersister payloadPersister, TimeSpan retentionPeriod)
        {
            _payloadPersister = payloadPersister;
            _retentionPeriod = retentionPeriod;
            
            __cancellation = new CancellationTokenSource();
        }

        /// <summary>
        /// Starts cleaning up payloads that are over the configured retention period.
        /// </summary>
        /// <returns></returns>
        public void Start()
        {
            Logger.Debug("Will clean up payloads older than: " + DateTimeOffset.UtcNow.Subtract(_retentionPeriod));

            Observable
                .Interval(TimeSpan.FromDays(1))
                .StartWith(0)
                .Do(_ =>
                {
                    _payloadPersister.CleanupPayloadsOlderThan(_retentionPeriod);
                    Logger.Trace("Clean up payloads older than: "
                                 + DateTimeOffset.UtcNow.Subtract(_retentionPeriod));
                })
                .Repeat()
                .ToTask(__cancellation.Token);
        }

        /// <summary>
        /// Stops cleaning up payloads periodically.
        /// </summary>
        public void Stop()
        {
            __cancellation.Cancel();
        }
    }
}