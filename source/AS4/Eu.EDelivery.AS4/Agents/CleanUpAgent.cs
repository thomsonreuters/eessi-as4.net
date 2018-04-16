using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Strategies.Database;
using NLog;

namespace Eu.EDelivery.AS4.Agents
{
    /// <summary>
    /// <see cref="IAgent"/> implementation that runs a Clean Up job every day.
    /// This job consists of deleting messages that are inserted older that the given retention period (local configuration settings specifies this in days).
    /// </summary>
    /// <seealso cref="IAgent" />
    public class CleanUpAgent : IAgent
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly Func<DatastoreContext> _storeExpression;
        private readonly TimeSpan _retentionPeriod;

        /// <summary>
        /// Initializes a new instance of the <see cref="CleanUpAgent"/> class.
        /// </summary>
        public CleanUpAgent() : this(() => new DatastoreContext(Config.Instance), Config.Instance.RetentionPeriod) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CleanUpAgent" /> class.
        /// </summary>
        /// <param name="storeExpression">The store expression.</param>
        /// <param name="retentionPeriod">The retention period.</param>
        public CleanUpAgent(Func<DatastoreContext> storeExpression, TimeSpan retentionPeriod)
        {
            _storeExpression = storeExpression;
            _retentionPeriod = retentionPeriod;
        }

        /// <summary>
        /// Gets the agent configuration.
        /// </summary>
        /// <value>The agent configuration.</value>
        public AgentConfig AgentConfig { get; } = new AgentConfig("Clean Up Agent");

        /// <summary>
        /// Starts the specified agent.
        /// </summary>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        public async Task Start(CancellationToken cancellation)
        {
            Logger.Info($"{AgentConfig.Name} Started!");
            Logger.Debug("Will clean up entries older than: " + DateTimeOffset.UtcNow.Subtract(_retentionPeriod));

            try
            {
                await Observable.Interval(TimeSpan.FromDays(1), TaskPoolScheduler.Default)
                    .StartWith(0)
                    .Do(_ => StartCleaningMessagesTables())
                    .ToTask(cancellation);
            }
            catch (TaskCanceledException)
            {
                Logger.Info($"{AgentConfig.Name} Stopped!");
            }
        }

        private void StartCleaningMessagesTables()
        {
            using (DatastoreContext context = _storeExpression())
            {
                var allowedOperations = new[]
                {
                    Operation.Delivered,
                    Operation.Forwarded,
                    Operation.Notified,
                    Operation.Sent,
                    Operation.NotApplicable,
                    Operation.Undetermined
                };

                foreach (string table in DatastoreTable.TablesByName.Keys.Where(k => !k.Equals("ReceptionAwareness")))
                {
                    context.NativeCommands
                           .BatchDeleteOverRetentionPeriod(table, _retentionPeriod, allowedOperations);
                }
            }
        }

        /// <summary>
        /// Stops this agent.
        /// </summary>
        public void Stop() { }
    }
}
