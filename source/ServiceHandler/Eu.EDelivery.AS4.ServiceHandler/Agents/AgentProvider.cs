using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
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
        private readonly IEnumerable<IAgent> _agents;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentProvider"/> class. Create a <see cref="AgentProvider"/> with the Core and Custom Agents
        /// </summary>
        /// <param name="config">The config.</param>
        public AgentProvider(IConfig config)
        {
            _config = config;
            _agents = new List<IAgent>();

            try
            {
                _agents =
                    CreateCustomAgents()
                        .Concat(CreateMinderAgents())
                        .Concat(new IAgent[]
                        {
                            new CleanUpAgent(
                                Registry.Instance.CreateDatastoreContext, 
                                config.RetentionPeriod),
                            new RetryAgent(
                                CreateRetryReceiver(config),
                                config.RetryPollingInterval,
                                Registry.Instance.CreateDatastoreContext)
                        });
            }
            catch (Exception exception)
            {
                LogManager.GetCurrentClassLogger().Error(exception.Message);
            }
            
        }

        private IEnumerable<Agent> CreateCustomAgents()
        {
            return _config.GetAgentsConfiguration().Select(CreateAgentBaseFromSettings);
        }

        private static Agent CreateAgentBaseFromSettings(AgentConfig config)
        {
            IReceiver receiver = new ReceiverBuilder().SetSettings(config.Settings.Receiver).Build();

            return new Agent(
                config: config,
                receiver: receiver,
                transformerConfig: config.Settings.Transformer,
                exceptionHandler: ExceptionHandlerRegistry.GetHandler(config.Type),
                stepConfiguration: config.Settings.StepConfiguration ?? GetDefaultStepConfigurationForAgentType(config.Type));
        }

        [ExcludeFromCodeCoverage]
        private IEnumerable<Agent> CreateMinderAgents()
        {
            return MinderAgentProvider.GetMinderSpecificAgentsFromConfig(_config);
        }

        private static IReceiver CreateRetryReceiver(IConfig config)
        {
            // TODO: this receiver is now created only for the retry agent, this creation should be moved closer to this agent

            var r = new DatastoreReceiver(
                Registry.Instance.CreateDatastoreContext,
                ctx => ctx.RetryReliability.Where(
                    rr => rr.Status == RetryStatus.Pending
                          && (rr.LastRetryTime.HasValue == false 
                              || DateTimeOffset.Now >= rr.LastRetryTime.Value.Add(rr.RetryInterval))).ToList());

            r.Configure(new DatastoreReceiverSettings(
                            tableName: "RetryReliability",
                            filter: "Status = 'Pending' AND Now >= LastRetryTime + RetryInterval",
                            updateField: "Status",
                            updateValue: "Busy",
                            pollingInterval: config.RetryPollingInterval));
            return r;
        }

        /// <summary>
        /// Return all the Registered <see cref="IAgent" /> Implementations
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IAgent> GetAgents()
        {
            return _agents.ToArray();
        }

        /// <summary>
        /// Gets the default implementation of the <see cref="StepConfiguration"/> for the given <paramref name="agentType"/>.
        /// </summary>
        /// <param name="agentType">Type of the agent.</param>
        /// <returns></returns>
        public static StepConfiguration GetDefaultStepConfigurationForAgentType(AgentType agentType)
        {
            return DefaultAgentStepRegistry.GetDefaultStepConfigurationFor(agentType);
        }

        /// <summary>
        /// Gets the default implementation of the <see cref="TransformerConfigEntry"/> for the given <paramref name="agentType"/>.
        /// </summary>
        /// <param name="agentType">Type of the agent.</param>
        /// <returns></returns>
        public static TransformerConfigEntry GetDefaultTransformerForAgentType(AgentType agentType)
        {
            return DefaultAgentTransformerRegistry.GetDefaultTransformerFor(agentType);
        }
    }
}