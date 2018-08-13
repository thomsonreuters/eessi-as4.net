using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps.Deliver;
using Eu.EDelivery.AS4.Steps.Forward;
using Eu.EDelivery.AS4.Steps.Notify;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.Steps.Submit;

namespace Eu.EDelivery.AS4.ServiceHandler.Agents
{
    internal static class DefaultAgentStepRegistry
    {
        private static readonly IDictionary<AgentType, StepConfiguration> StepConfiguration =
            new Dictionary<AgentType, StepConfiguration>();

        #region initialization

        static DefaultAgentStepRegistry()
        {
            StepConfiguration.Add(AgentType.Submit,
                new StepConfiguration
                {
                    NormalPipeline = new[]
                    {
                        new Step { Type = typeof(RetrieveSendingPModeStep).AssemblyQualifiedName },
                        new Step { Type = typeof(DynamicDiscoveryStep).AssemblyQualifiedName },
                        new Step { Type = typeof(CreateAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(StoreAS4MessageStep).AssemblyQualifiedName }
                    }
                });

            StepConfiguration.Add(AgentType.OutboundProcessing,
                new StepConfiguration
                {
                    NormalPipeline = new[]
                    {
                        new Step { Type = typeof(CompressAttachmentsStep).AssemblyQualifiedName },
                        new Step { Type = typeof(SignAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(EncryptAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(SetMessageToBeSentStep).AssemblyQualifiedName }
                    }
                });

            StepConfiguration.Add(AgentType.PullSend,
                new StepConfiguration
                {
                    NormalPipeline = new[]
                    {
                        new Step { Type = typeof(VerifySignatureAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(VerifyPullRequestAuthorizationStep).AssemblyQualifiedName },
                        new Step { Type = typeof(SaveReceivedMessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(DeterminePModesStep).AssemblyQualifiedName },
                        new Step { Type = typeof(UpdateReceivedAS4MessageBodyStep).AssemblyQualifiedName },
                        new Step { Type = typeof(SelectUserMessageToSendStep).AssemblyQualifiedName }
                    }
                });

            StepConfiguration.Add(AgentType.PushSend,
                new StepConfiguration
                {
                    NormalPipeline = new[]
                    {
                        new Step { Type = typeof(SendAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(SaveReceivedMessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(DeterminePModesStep).AssemblyQualifiedName },
                        new Step { Type = typeof(VerifySignatureAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(UpdateReceivedAS4MessageBodyStep).AssemblyQualifiedName }
                    },
                    ErrorPipeline = new[]
                    {
                        new Step { Type = typeof(LogReceivedProcessingErrorStep).AssemblyQualifiedName }
                    }
                });

            StepConfiguration.Add(AgentType.Receive,
                new StepConfiguration
                {
                    NormalPipeline = new[]
                    {
                        new Step { Type = typeof(SaveReceivedMessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(DeterminePModesStep).AssemblyQualifiedName },
                        new Step { Type = typeof(ValidateAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(DecryptAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(VerifySignatureAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(DecompressAttachmentsStep).AssemblyQualifiedName },
                        new Step { Type = typeof(UpdateReceivedAS4MessageBodyStep).AssemblyQualifiedName },
                        new Step { Type = typeof(CreateAS4ReceiptStep).AssemblyQualifiedName },
                        new Step { Type = typeof(SignAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(SendAS4SignalMessageStep).AssemblyQualifiedName }
                    },
                    ErrorPipeline = new[]
                    {
                        new Step { Type = typeof(CreateAS4ErrorStep).AssemblyQualifiedName },
                        new Step { Type = typeof(SignAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(SendAS4SignalMessageStep).AssemblyQualifiedName }
                    }
                });

            StepConfiguration.Add(AgentType.PullReceive,
                new StepConfiguration
                {

                    NormalPipeline = new[]
                    {
                        new Step { Type = typeof(SignAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(SendAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(SaveReceivedMessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(DeterminePModesStep).AssemblyQualifiedName },
                        new Step { Type = typeof(ValidateAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(DecryptAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(VerifySignatureAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(DecompressAttachmentsStep).AssemblyQualifiedName },
                        new Step { Type = typeof(UpdateReceivedAS4MessageBodyStep).AssemblyQualifiedName },
                        new Step { Type = typeof(CreateAS4ReceiptStep).AssemblyQualifiedName },
                        new Step { Type = typeof(SignAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(SendAS4SignalMessageStep).AssemblyQualifiedName }
                    },
                    ErrorPipeline = new[]
                    {
                        new Step { Type = typeof(CreateAS4ErrorStep).AssemblyQualifiedName },
                        new Step { Type = typeof(SignAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(SendAS4SignalMessageStep).AssemblyQualifiedName }
                    }
                });

            StepConfiguration.Add(AgentType.Forward,
                new StepConfiguration
                {
                    NormalPipeline = new[]
                    {
                        new Step { Type = typeof(DetermineRoutingStep).AssemblyQualifiedName },
                        new Step { Type = typeof(DynamicDiscoveryStep).AssemblyQualifiedName },
                        new Step { Type = typeof(CreateForwardMessageStep).AssemblyQualifiedName }
                    }
                });

            StepConfiguration.Add(AgentType.Deliver,
                new StepConfiguration
                {
                    NormalPipeline = new[]
                    {
                        new Step { Type = typeof(UploadAttachmentsStep).AssemblyQualifiedName },
                        new Step { Type = typeof(CreateDeliverEnvelopeStep).AssemblyQualifiedName },
                        new Step { Type = typeof(SendDeliverMessageStep).AssemblyQualifiedName }
                    }
                });

            StepConfiguration.Add(AgentType.Notify,
                new StepConfiguration
                {
                    NormalPipeline = new[]
                    {
                        new Step { Type = typeof(SendNotifyMessageStep).AssemblyQualifiedName },
                    }
                });
        }

        #endregion

        internal static StepConfiguration GetDefaultStepConfigurationFor(AgentType agentType)
        {
            if (StepConfiguration.ContainsKey(agentType) == false)
            {
                throw new NotSupportedException($"There is no default StepConfiguration available for agent-type {agentType}");
            }

            return StepConfiguration[agentType];
        }
    }
}