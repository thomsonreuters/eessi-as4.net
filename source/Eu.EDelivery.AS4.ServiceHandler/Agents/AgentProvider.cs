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
        private readonly IEnumerable<IAgent> _agents;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentProvider"/> class. Create a <see cref="AgentProvider"/> with the Core and Custom Agents
        /// </summary>
        /// <param name="agents"></param>
        private AgentProvider(IEnumerable<IAgent> agents)
        {
            if (agents == null)
            {
                throw new ArgumentNullException(nameof(agents));
            }

            _agents = agents;
        }

        /// <summary>
        /// Creates an <see cref="AgentProvider"/> based on the configured <paramref name="config"/> using the initialized <paramref name="registry"/>.
        /// </summary>
        /// <param name="config">Configuration to build the <see cref="IAgent"/> implementations.</param>
        /// <param name="registry">Registration of different implementations.</param>
        /// <returns></returns>
        public static AgentProvider BuildFromConfig(IConfig config, IRegistry registry)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            if (!config.IsInitialized)
            {
                throw new InvalidOperationException(
                    "AgentProvider requires an initialized IConfig implementation to provide Agents");
            }

            if (!registry.IsInitialized)
            {
                throw new InvalidOperationException(
                    "AgentProvider requires an initialized IRegistry implementation to provide Agents");
            }

            IEnumerable<AgentConfig> agentConfigs = config.GetAgentsConfiguration();
            if (agentConfigs == null)
            {
                throw new ArgumentNullException(
                    nameof(agentConfigs),
                    @"AgentProvider requires a collection of AgentConfig instances from the IConfig.GetAgentsConfiguration() call");
            }

            if (agentConfigs.Any(c => c is null))
            {
                throw new ArgumentNullException(
                    nameof(agentConfigs),
                    @"Fails to create IAgent implementations: one or more AgentConfig instances of the IConfig.GetAgentsConfiguration() call is invalid");
            }

            try
            {
                IAgent[] agents =
                    agentConfigs
                        .Select(CreateAgentBaseFromSettings)
                        .Concat(CreateMinderAgents(config))
                        .Concat(new IAgent[]
                        {
                            new CleanUpAgent(
                                registry.CreateDatastoreContext,
                                config.RetentionPeriod),
                            new RetryAgent(
                                CreateRetryReceiver(config, registry),
                                registry.CreateDatastoreContext)
                        })
                        .ToArray();

                return new AgentProvider(agents);
            }
            catch (Exception exception)
            {
                Logger.Fatal(exception.Message);
                throw;
            }
        }

        private static Agent CreateAgentBaseFromSettings(AgentConfig config)
        {
            string agentLogTag = $"{config.Type} Agent {config.Name}";
            if (config.Settings == null)
            {
                throw new ArgumentNullException(nameof(config.Settings), $@"{agentLogTag} hasn't got valid Settings");
            }

            if (config.Settings.Receiver?.Type == null)
            {
                throw new ArgumentNullException(nameof(config.Settings.Receiver.Type), $@"{agentLogTag} hasn't got a Receiver.Type");
            }

            if (config.Settings.Transformer?.Type == null)
            {
                throw new ArgumentNullException(nameof(config.Settings.Transformer.Type), $@"{agentLogTag} hasn't got a Transformer.Type");
            }

            if (config.Settings.StepConfiguration?.NormalPipeline != null
                && config.Settings.StepConfiguration.NormalPipeline.Count(s => s?.Type == null) > 0)
            {
                throw new ArgumentNullException(
                    nameof(config.Settings.StepConfiguration.NormalPipeline),
                    $@"{agentLogTag} has one ore more Steps in the NormalPipeline without a Type");
            }

            if (config.Settings.StepConfiguration?.ErrorPipeline != null
                && config.Settings.StepConfiguration.ErrorPipeline?.Count(s => s?.Type == null) > 0)
            {
                throw new ArgumentNullException(
                    nameof(config.Settings.StepConfiguration.ErrorPipeline),
                    $@"{agentLogTag} has one or more Steps in the ErrorPipeline without a Type");
            }

            IReceiver receiver = 
                new ReceiverBuilder()
                    .SetSettings(config.Settings.Receiver)
                    .Build();

            return new Agent(
                config: config,
                receiver: receiver,
                transformerConfig: config.Settings.Transformer,
                exceptionHandler: ExceptionHandlerRegistry.GetHandler(config.Type),
                stepConfiguration: config.Settings.StepConfiguration ?? GetDefaultStepConfigurationForAgentType(config.Type));
        }

        [ExcludeFromCodeCoverage]
        private static IEnumerable<Agent> CreateMinderAgents(IConfig config)
        {
            return MinderAgentProvider.GetMinderSpecificAgentsFromConfig(config);
        }

        private static IReceiver CreateRetryReceiver(IConfig config, IRegistry registry)
        {
            var r = new DatastoreReceiver(
                registry.CreateDatastoreContext,
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
            return _agents;
        }

        /// <summary>
        /// Gets the default implementation of the <see cref="Receiver"/> for a given <paramref name="agentType"/>.
        /// </summary>
        /// <param name="agentType">Type of the agent.</param>
        /// <returns></returns>
        public static Receiver GetDefaultReceiverForAgentType(AgentType agentType)
        {
            return DefaultAgentReceiverRegistry.GetDefaultReceiverFor(agentType);
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

        /// <summary>
        /// Gets the default implementation of the <see cref="StepConfiguration"/> for the given <paramref name="agentType"/>.
        /// </summary>
        /// <param name="agentType">Type of the agent.</param>
        /// <returns></returns>
        public static StepConfiguration GetDefaultStepConfigurationForAgentType(AgentType agentType)
        {
            return DefaultAgentStepRegistry.GetDefaultStepConfigurationFor(agentType);
        }
    }
}