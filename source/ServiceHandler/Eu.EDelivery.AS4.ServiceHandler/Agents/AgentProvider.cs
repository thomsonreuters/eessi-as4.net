using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.ServiceHandler.Builder;
using Eu.EDelivery.AS4.Steps.Common;
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
            catch (AS4Exception exception)
            {
                _logger.Error(exception.Message);
            }
        }

        private void AddCustomAgentsToProvider()
        {
            foreach (SettingsAgent settingAgent in _config.GetSettingsAgents())
            {
                IAgent agent = GetAgentFromSettings(settingAgent);

                _agents.Add(agent);
            }
        }

        private static IAgent GetAgentFromSettings(SettingsAgent agent)
        {
            if (agent == null)
            {
                throw new ArgumentNullException(nameof(agent));
            }

            IReceiver receiver = new ReceiverBuilder().SetSettings(agent.Receiver).Build();

            return new Agent(new AgentConfig(agent.Name), receiver, agent.Transformer, agent.Steps);
        }

        [ExcludeFromCodeCoverage]
        private void AddMinderAgentsToProvider()
        {
            IEnumerable<SettingsMinderAgent> minderTestAgents = _config.GetEnabledMinderTestAgents();

            foreach (SettingsMinderAgent agent in minderTestAgents)
            {
                _agents.Add(CreateMinderTestAgent(agent.Url, agent.Transformer));
            }
        }

        [ExcludeFromCodeCoverage]
        private static Agent CreateMinderTestAgent(string url, Transformer transformerConfig)
        {
            var receiver = new HttpReceiver();

            receiver.Configure(new[] { new Setting("Url", url) });

            return new Agent(
                new AgentConfig("Minder Submit/Receive Agent"),
                receiver,
                transformerConfig,
                CreateMinderSubmitReceiveStepConfig());
        }

        [ExcludeFromCodeCoverage]
        private static ConditionalStepConfig CreateMinderSubmitReceiveStepConfig()
        {
            Func<InternalMessage, bool> isSubmitMessage =
                m =>
                    m.SubmitMessage.Collaboration?.Action?.Equals("Submit", StringComparison.OrdinalIgnoreCase) ?? false;

            Model.Internal.Steps submitStepConfig = CreateSubmitStep();
            Model.Internal.Steps receiveStepConfig = CreateReceiveStep();

            return new ConditionalStepConfig(isSubmitMessage, submitStepConfig, receiveStepConfig);
        }

        [ExcludeFromCodeCoverage]
        private static Model.Internal.Steps CreateSubmitStep()
        {
            var s = new Model.Internal.Steps
            {
                Decorator = typeof(OutExceptionStepDecorator).AssemblyQualifiedName,
                Step =
                    new[]
                    {
                        new Step {Type = typeof(StoreAS4MessageStep).AssemblyQualifiedName},
                        new Step {Type = typeof(CreateAS4ReceiptStep).AssemblyQualifiedName},
                    }
            };

            return s;
        }

        [ExcludeFromCodeCoverage]
        private static Model.Internal.Steps CreateReceiveStep()
        {
            return new Model.Internal.Steps
            {
                Decorator = typeof(ReceiveExceptionStepDecorator).AssemblyQualifiedName,
                Step =
                    new[]
                    {
                        new Step {Type = typeof(SaveReceivedMessageStep).AssemblyQualifiedName},
                        new Step {Type = typeof(DeterminePModesStep).AssemblyQualifiedName},
                        new Step {Type = typeof(DecryptAS4MessageStep).AssemblyQualifiedName},
                        new Step {Type = typeof(VerifySignatureAS4MessageStep).AssemblyQualifiedName},
                        new Step {Type = typeof(DecompressAttachmentsStep).AssemblyQualifiedName},
                        new Step {Type = typeof(UpdateReceivedAS4MessageBodyStep).AssemblyQualifiedName},
                        new Step {Type = typeof(CreateAS4ReceiptStep).AssemblyQualifiedName},
                        new Step {Type = typeof(StoreAS4ReceiptStep).AssemblyQualifiedName},
                        new Step {Type = typeof(SignAS4MessageStep).AssemblyQualifiedName},
                        new Step {Type = typeof(SendAS4ReceiptStep).AssemblyQualifiedName},
                        new Step {UnDecorated = true, Type = typeof(CreateAS4ErrorStep).AssemblyQualifiedName},
                        new Step {UnDecorated = true, Type = typeof(SignAS4MessageStep).AssemblyQualifiedName},
                        new Step {UnDecorated = true, Type = typeof(SendAS4ErrorStep).AssemblyQualifiedName}
                    }
            };
        }
    }
}