using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Exceptions.Handlers;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.ServiceHandler.Builder;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.Steps.Submit;
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
        /// Initializes a new instance of the <see cref="AgentProvider"/> class. Create a <see cref="AgentProvider"/> with the Core and Custom Agents
        /// </summary>
        /// <param name="config">The config.</param>
        public AgentProvider(IConfig config)
        {
            _config = config;
            _logger = LogManager.GetCurrentClassLogger();
            _agents = new Collection<IAgent>();

            TryAddCustomAgentsToProvider();
        }

        /// <summary>
        /// Return all the Registered <see cref="IAgent" /> Implementations
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IAgent> GetAgents()
        {
            return _agents;
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

        private static AgentBase CreateAgentBaseFromSettings(AgentConfig config)
        {
            IReceiver receiver = new ReceiverBuilder().SetSettings(config.Settings.Receiver).Build();

            return new AgentBase(
                name: config.Name,
                receiver: receiver,
                transformerConfig: config.Settings.Transformer,
                exceptionHandler: ExceptionHandlerRegistry.GetHandler(config.Type),
                pipelineConfig: (config.Settings.NormalPipeline, config.Settings.ErrorPipeline));
        }

        [ExcludeFromCodeCoverage]
        private void AddMinderAgentsToProvider()
        {
            IEnumerable<SettingsMinderAgent> minderTestAgents = _config.GetEnabledMinderTestAgents();

            foreach (SettingsMinderAgent agent in minderTestAgents)
            {
                _agents.Add(CreateMinderTestAgent(agent.Url, agent.UseLogging, agent.Transformer));
            }
        }

        [ExcludeFromCodeCoverage]
        private static AgentBase CreateMinderTestAgent(string url, bool useLogging, Transformer transformerConfig)
        {
            var receiver = new HttpReceiver();

            receiver.Configure(new[] { new Setting("Url", url), new Setting("UseLogging", useLogging.ToString()) });

            return new AgentBase(
                "Minder Submit/Receive Agent",
                receiver,
                transformerConfig,
                new MinderExceptionHandler(), 
                (CreateMinderHappyFlow(), CreateReceiveUnhappyFlow()));
        }

        [ExcludeFromCodeCoverage]
        private static ConditionalStepConfig CreateMinderHappyFlow()
        {
            Func<MessagingContext, bool> isSubmitMessage = m => m.Mode == MessagingContextMode.Submit;                

            Model.Internal.Steps submitStepConfig = CreateSubmitStep();
            Model.Internal.Steps receiveStepConfig = CreateReceiveHappyFlow();

            return new ConditionalStepConfig(isSubmitMessage, submitStepConfig, receiveStepConfig);
        }

        private static ConditionalStepConfig CreateReceiveUnhappyFlow()
        {
            Func<MessagingContext, bool> isSubmitMessage = m => m.Mode == MessagingContextMode.Submit;

            var submitStepConfig = new Model.Internal.Steps { Step = new Step[0] };
            Model.Internal.Steps receiveStepConfig = CreateUnhappyReceiveFlow();

            return new ConditionalStepConfig(isSubmitMessage, submitStepConfig, receiveStepConfig);
        }

        [ExcludeFromCodeCoverage]
        private static Model.Internal.Steps CreateSubmitStep()
        {
            return new Model.Internal.Steps
            {
                Step =
                    new[]
                    {
                        new Step {Type = typeof(StoreAS4MessageStep).AssemblyQualifiedName},
                        new Step {Type = typeof(CreateAS4ReceiptStep).AssemblyQualifiedName}
                    }
            };
        }

        [ExcludeFromCodeCoverage]
        private static Model.Internal.Steps CreateReceiveHappyFlow()
        {
            return new Model.Internal.Steps
            {
                Step =
                    new[]
                    {
                        new Step {Type = typeof(SaveReceivedMessageStep).AssemblyQualifiedName},
                        new Step {Type = typeof(DeterminePModesStep).AssemblyQualifiedName},
                        new Step {Type = typeof(ValidateAS4MessageStep).AssemblyQualifiedName},
                        new Step {Type = typeof(DecryptAS4MessageStep).AssemblyQualifiedName},
                        new Step {Type = typeof(VerifySignatureAS4MessageStep).AssemblyQualifiedName},
                        new Step {Type = typeof(DecompressAttachmentsStep).AssemblyQualifiedName},
                        new Step {Type = typeof(UpdateReceivedAS4MessageBodyStep).AssemblyQualifiedName},
                        new Step {Type = typeof(CreateAS4ReceiptStep).AssemblyQualifiedName},
                        new Step {Type = typeof(StoreAS4ReceiptStep).AssemblyQualifiedName},
                        new Step {Type = typeof(SignAS4MessageStep).AssemblyQualifiedName},
                        new Step {Type = typeof(SendAS4SignalMessageStep).AssemblyQualifiedName},
                        
                    }
            };
        }

        private static Model.Internal.Steps CreateUnhappyReceiveFlow()
        {
            return new Model.Internal.Steps
            {
                Step =
                    new[]
                    {
                        new Step {Type = typeof(CreateAS4ErrorStep).AssemblyQualifiedName},
                        new Step {Type = typeof(SignAS4MessageStep).AssemblyQualifiedName},
                        new Step {Type = typeof(SendAS4MessageStep).AssemblyQualifiedName}
                    }
            };
        }
    }
}