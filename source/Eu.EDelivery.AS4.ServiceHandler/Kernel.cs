using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.ServiceHandler.Agents;
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
        private readonly IConfig _config;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="Kernel" /> class.
        /// </summary>
        /// <param name="agents">The agents.</param>
        /// <param name="config">The configuration.</param>
        internal Kernel(IEnumerable<IAgent> agents, IConfig config)
        {
            if (agents == null)
            {
                throw new ArgumentNullException(nameof(agents));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _agents = agents;
            _config = config;
        }

        /// <summary>
        /// Create an <see cref="Kernel" /> instance from a given settings file name.
        /// </summary>
        /// <param name="settings">The file name in the '.\config\' folder of the settings to use during the initialization (default: 'settings.xml').</param>
        /// <returns></returns>
        public static Kernel CreateFromSettings(string settings = "settings.xml")
        {
            if (string.IsNullOrWhiteSpace(settings))
            {
                throw new ArgumentException(
                    @"Settings file name cannot be null or whitespace (default: 'settings.xml').", 
                    nameof(settings));
            }

            Config config = Config.Instance;
            Registry registry = Registry.Instance;

            config.Initialize(settings);
            if (!config.IsInitialized)
            {
                throw new InvalidOperationException(
                    "Cannot create Kernel: couldn't correctly initialize the configuration");
            }

            registry.Initialize(config);
            if (!registry.IsInitialized)
            {
                throw new InvalidOperationException(
                    "Cannot create Kernel: couldn't correctly initialize the registry");
            }

            var agentProvider = AgentProvider.BuildFromConfig(config, registry);
            return new Kernel(agentProvider.GetAgents(), config);
        }

        /// <summary>
        /// Starting Kernel > starting all Agents
        /// </summary>
        /// <param name="cancellationToken">Cancel the Kernel if needed</param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_agents.Any())
            {
                Logger.Warn("Will not start Kernel: no IAgent implementations has been set to the Kernel");
                return;
            }

            AS4Mapper.Initialize();

            try
            {
                using (var context = new DatastoreContext(_config))
                {
                    await context.NativeCommands.CreateDatabase();
                }
            }
            catch (Exception exception)
            {
                Logger.Fatal($"An error occured while migrating the database: {exception.Message}");
                Logger.Trace(exception.StackTrace);

                if (exception.InnerException != null)
                {
                    Logger.Fatal(exception.InnerException.Message);
                    Logger.Trace(exception.InnerException.StackTrace);
                }

                return;

            }

            Logger.Trace("Starting...");
            Task task = Task.WhenAll(_agents.Select(c => c.Start(cancellationToken)).ToArray());
            Logger.Trace("Started!");

            await task;

            CloseAgents();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
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