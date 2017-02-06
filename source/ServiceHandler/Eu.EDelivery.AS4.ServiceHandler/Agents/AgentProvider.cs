using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.ServiceHandler.Builder;
using Eu.EDelivery.AS4.Steps.Common;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.Steps.Submit;
using Eu.EDelivery.AS4.Transformers;
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

                var minderUrls = this._config.GetUrlsForEnabledMinderTestAgents();

                foreach (var url in minderUrls)
                {
                    this._agents.Add(CreateMinderTestAgent(url));
                }
            }
            catch (AS4Exception exception)
            {
                this._logger.Error(exception.Message);
            }
        }

        private void AddCustomAgentsToProvider()
        {
            foreach (SettingsAgent settingAgent in this._config.GetSettingsAgents())
            {
                IAgent agent = GetAgentFromSettings(settingAgent);
                agent.AgentConfig = new AgentConfig(settingAgent.Name);

                this._agents.Add(agent);
            }
        }
        
        private static IAgent GetAgentFromSettings(SettingsAgent agent)
        {
            IReceiver receiver = new ReceiverBuilder().SetSettings(agent.Receiver).Build();
            
            return new Agent(receiver, agent.Transformer, agent.Steps);
        }

        private static Agent CreateMinderTestAgent(string url)
        {
            var receiver = new HttpReceiver();
            receiver.Configure(new Dictionary<string, string> { ["Url"] = url }); 

            var transformerConfig = new Model.Internal.Transformer()
            {
                Type = typeof(AS4MessageTransformer).AssemblyQualifiedName
            };

            return new Agent(receiver, transformerConfig, CreateMinderSubmitReceiveStepConfig());
        }

        private static ConditionalStepConfig CreateMinderSubmitReceiveStepConfig()
        {

            Func<AS4Message, bool> isSubmitMessage =
                m => m?.PrimaryUserMessage?.CollaborationInfo?.Action?.Equals("Submit") == true;

            var submitStepConfig = CreateSubmitStep();
            var receiveStepConfig = CreateReceiveStep();

            return new ConditionalStepConfig(isSubmitMessage, submitStepConfig, receiveStepConfig);
        }

        private static Model.Internal.Steps CreateSubmitStep()
        {
            var s = new Model.Internal.Steps()
            {
                Decorator = typeof(OutExceptionStepDecorator).AssemblyQualifiedName,
                Step = new Step[]
                {
                    new Step { Type = typeof(MinderAssembleAS4MessageStep).AssemblyQualifiedName },
                    new Step { Type = typeof(MinderRetrieveSendingPModeStep).AssemblyQualifiedName },
                    new Step { Type = typeof(StoreAS4MessageStep).AssemblyQualifiedName },
                    new Step { Type = typeof(MinderCreateAS4ReceiptStep).AssemblyQualifiedName},
                }
            };

            return s;
        }

        private static Model.Internal.Steps CreateReceiveStep()
        {
            return new Model.Internal.Steps()
            {
                Decorator = typeof(ReceiveExceptionStepDecorator).AssemblyQualifiedName,
                Step = new Step[]
                {
                    new Step { Type = typeof(DeterminePModesStep).AssemblyQualifiedName },
                    new Step { Type = typeof(MinderDecryptAS4MessageStep).AssemblyQualifiedName },
                    new Step { Type = typeof(VerifySignatureAS4MessageStep).AssemblyQualifiedName },
                    new Step { Type = typeof(DecompressAttachmentsStep).AssemblyQualifiedName },
                    new Step { Type = typeof(ReceiveUpdateDatastoreStep).AssemblyQualifiedName},
                    new Step { Type = typeof(CreateAS4ReceiptStep).AssemblyQualifiedName },
                    new Step { Type = typeof(StoreAS4ReiptStep).AssemblyQualifiedName},
                    new Step { Type = typeof(MinderSignAS4MessageStep).AssemblyQualifiedName },
                    new Step { Type = typeof(SendAS4ReceiptStep).AssemblyQualifiedName },
                    new Step { UnDecorated = true,Type = typeof(CreateAS4ErrorStep).AssemblyQualifiedName },
                    new Step { UnDecorated = true, Type=typeof(MinderSignAS4MessageStep).AssemblyQualifiedName},
                }
            };
        }
    }
}