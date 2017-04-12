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
        private readonly IEnumerable<IAgent> _agents;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Create Startup Kernel
        /// </summary>
        /// <param name="agents"></param>
        public Kernel(IEnumerable<IAgent> agents)
        {
            if (agents == null)
            {
                Logger.Error("Kernel hasn't got IAgent implementations, so cannot be started");
            }

            _agents = agents;
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

            using (var context = new DatastoreContext(Config.Instance))
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
            foreach (IAgent agent in _agents)
            {
                var disposableAgent = agent as IDisposable;
                disposableAgent?.Dispose();
            }
        }
    }
}