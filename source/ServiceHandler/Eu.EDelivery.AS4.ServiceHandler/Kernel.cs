using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Fe;
using NLog;

namespace Eu.EDelivery.AS4.ServiceHandler
{
    /// <summary>
    /// Start point for AS4 Connection
    /// Wrapper for the Channels
    /// </summary>
    public class Kernel
    {
        private readonly IEnumerable<IAgent> _agents;
        private readonly ILogger _logger;

        /// <summary>
        /// Create Startup Kernel
        /// </summary>
        /// <param name="agents"></param>
        public Kernel(IEnumerable<IAgent> agents)
        {
            if (agents == null) this._logger.Error("Kernel hasn't got IAgent implementations, so cannot be started");

            this._agents = agents;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Starting Kernel > starting all Agents
        /// </summary>
        /// <param name="cancellationToken">Cancel the Kernel if needed</param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Program.Main(null);
            if (this._agents == null) return;

            this._logger.Debug("Starting...");
            Task task = Task.WhenAll(this._agents.Select(c => c.Start(cancellationToken)).ToArray());
            this._logger?.Debug("Started!");

            await task;
        }
    }
}