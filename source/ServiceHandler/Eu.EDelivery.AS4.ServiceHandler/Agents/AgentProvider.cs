using System.Collections.Generic;
using System.Collections.ObjectModel;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.ServiceHandler.Builder;
using NLog;

namespace Eu.EDelivery.AS4.ServiceHandler.Agents
{
    /// <summary>
    /// Agent Provider/Manager Resposibility:
    /// manage the registered Agents (default and extendible)
    /// </summary>
    public class AgentProvider
    {
        private readonly IConfig _config;
        private readonly ICollection<IAgent> _agents;
        private readonly ILogger _logger;

        /// <summary>
        /// Create a <see cref="AgentProvider" />
        /// with the Core and Custom Agents
        /// </summary>
        public AgentProvider(IConfig config)
        {
            this._config = config;
            this._logger = LogManager.GetCurrentClassLogger();
            this._agents = new Collection<IAgent>();

            TryAddCustomAgentsToProvider();
        }

        /// <summary>
        /// Return all the Registered <see cref="IAgent" /> Implementations
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IAgent> GetAgents()
        {
            return this._agents;
        }

        private void TryAddCustomAgentsToProvider()
        {
            try
            {
                AddCustomAgentsToProvider();
            }
            catch (AS4Exception exception)
            {
                this._logger.Error(exception.Message);
            }
        }

        private void AddCustomAgentsToProvider()
        {
            foreach (SettingsAgent settingAgent in this._config.GetSettingsAgents())
                AddCustomAgentToProvider(settingAgent);
        }

        private void AddCustomAgentToProvider(SettingsAgent settingAgent)
        {
            IAgent agent = GetAgentFromSettings(settingAgent);
            agent.AgentConfig = new AgentConfig(settingAgent.Name);

            this._agents.Add(agent);
        }

        private static IAgent GetAgentFromSettings(SettingsAgent agent)
        {
            IReceiver receiver = new ReceiverBuilder().SetSettings(agent.Receiver).Build();
            
            return new Agent(receiver, agent.Transformer, agent.Steps);
        }
    }
}