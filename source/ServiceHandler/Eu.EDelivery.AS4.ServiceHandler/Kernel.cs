using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Singletons;
using NLog;

namespace Eu.EDelivery.AS4.ServiceHandler
{
    /// <summary>
    /// Start point for AS4 Connection
    /// Wrapper for the Channels
    /// </summary>
    public sealed class Kernel : IDisposable
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IEnumerable<IAgent> _agents;
        private readonly IConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="Kernel"/> class. 
        /// </summary>
        /// <param name="agents"></param>
        public Kernel(IEnumerable<IAgent> agents)
        {
            if (agents == null)
            {
                Logger.Error("Kernel hasn't got IAgent implementations, so cannot be started");
            }

            _agents = agents;
            _config = Config.Instance;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Kernel" /> class.
        /// </summary>
        /// <param name="agents">The agents.</param>
        /// <param name="config">The configuration.</param>
        public Kernel(IEnumerable<IAgent> agents, IConfig config) : this(agents)
        {
            _config = config;
        }

        /// <summary>
        /// Starting Kernel > starting all Agents
        /// </summary>
        /// <param name="cancellationToken">Cancel the Kernel if needed</param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_agents == null)
            {
                return;
            }

            using (var context = new DatastoreContext(_config))
            {
                try
                {
                    if (context.Database.EnsureCreated())
                    {
                        Logger.Info("Datastore did not exist and has been created.");
                    }
                }
                catch (Exception exception)
                {
                    Logger.Fatal($"Datastore failed to create or already created: {exception.Message}");
                    return;
                }
            }

            Logger.Debug("Starting...");
            Task task = Task.WhenAll(_agents.Select(c => c.Start(cancellationToken)).ToArray());
            Logger?.Debug("Started!");

            await task;

            CloseAgents();
        }

        public void Dispose()
        {
           CloseAgents();
        }

        private void CloseAgents()
        {
            if (_agents == null)
            {
                return;
            }

            foreach (IAgent agent in _agents)
            {
                var disposableAgent = agent as IDisposable;
                disposableAgent?.Dispose();
            }
        }
    }
}