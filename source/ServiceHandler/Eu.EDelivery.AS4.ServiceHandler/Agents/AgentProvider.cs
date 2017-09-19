using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions.Handlers;
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
        private readonly List<IAgent> _agents;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentProvider"/> class. Create a <see cref="AgentProvider"/> with the Core and Custom Agents
        /// </summary>
        /// <param name="config">The config.</param>
        public AgentProvider(IConfig config)
        {
            _config = config;
            _logger = LogManager.GetCurrentClassLogger();
            _agents = new List<IAgent>();

            TryAddCustomAgentsToProvider();
        }

        /// <summary>
        /// Return all the Registered <see cref="IAgent" /> Implementations
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IAgent> GetAgents()
        {
            return _agents.ToArray();
        }

        private void TryAddCustomAgentsToProvider()
        {
            try
            {
                AddCustomAgentsToProvider();
                AddMinderAgentsToProvider();
            }
            catch (Exception exception)
            {
                _logger.Error(exception.Message);
            }
        }

        private void AddCustomAgentsToProvider()
        {
            foreach (AgentConfig config in _config.GetAgentsConfiguration())
            {
                _agents.Add(CreateAgentBaseFromSettings(config));
            }
        }

        private static Agent CreateAgentBaseFromSettings(AgentConfig config)
        {
            IReceiver receiver = new ReceiverBuilder().SetSettings(config.Settings.Receiver).Build();

            return new Agent(
                name: config.Name,
                receiver: receiver,
                transformerConfig: config.Settings.Transformer,
                exceptionHandler: ExceptionHandlerRegistry.GetHandler(config.Type),
                stepConfiguration: config.Settings.StepConfiguration ?? GetDefaultStepConfigurationForAgentType(config.Type));
        }

        public static StepConfiguration GetDefaultStepConfigurationForAgentType(AgentType agentType)
        {
            return DefaultAgentStepRegistry.GetDefaultStepConfigurationFor(agentType);
        }

        public static Transformer GetDefaultTransformerForAgentType(AgentType agentType)
        {
            return DefaultAgentTransformerRegistry.GetDefaultTransformerFor(agentType);
        }

        [ExcludeFromCodeCoverage]
        private void AddMinderAgentsToProvider()
        {
            var minderTestAgents = MinderAgentProvider.GetMinderSpecificAgentsFromConfig(_config);

            _agents.AddRange(minderTestAgents);
        }
    }
}