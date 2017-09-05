using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions.Handlers;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.Steps.Submit;

namespace Eu.EDelivery.AS4.ServiceHandler.Agents
{
    [ExcludeFromCodeCoverage]
    internal static class MinderAgentProvider
    {
        internal static IEnumerable<Agent> GetMinderSpecificAgentsFromConfig(IConfig config)
        {
            IEnumerable<SettingsMinderAgent> minderTestAgents = config.GetEnabledMinderTestAgents();

            foreach (SettingsMinderAgent agent in minderTestAgents)
            {
                yield return CreateMinderTestAgent(agent.Url, agent.UseLogging, agent.Transformer);
            }
        }

        [ExcludeFromCodeCoverage]
        private static Agent CreateMinderTestAgent(string url, bool useLogging, Transformer transformerConfig)
        {
            var receiver = new HttpReceiver();

            receiver.Configure(new[] { new Setting("Url", url), new Setting("UseLogging", useLogging.ToString()) });

            return new Agent(
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

            Step[] submitStepConfig = CreateSubmitSteps();
            Step[] receiveStepConfig = CreateReceiveHappyFlowSteps();

            return new ConditionalStepConfig(isSubmitMessage, submitStepConfig, receiveStepConfig);
        }

        [ExcludeFromCodeCoverage]
        private static ConditionalStepConfig CreateReceiveUnhappyFlow()
        {
            Func<MessagingContext, bool> isSubmitMessage = m => m.Mode == MessagingContextMode.Submit;

            var submitStepConfig = new Step[0];
            Step[] receiveStepConfig = CreateReceiveUnhappyFlowSteps();

            return new ConditionalStepConfig(isSubmitMessage, submitStepConfig, receiveStepConfig);
        }

        [ExcludeFromCodeCoverage]
        private static Step[] CreateSubmitSteps()
        {
            return new[]
            {
                new Step {Type = typeof(StoreAS4MessageStep).AssemblyQualifiedName},
                new Step {Type = typeof(CreateAS4ReceiptStep).AssemblyQualifiedName}
            };
        }

        [ExcludeFromCodeCoverage]
        private static Step[] CreateReceiveHappyFlowSteps()
        {
            return new[]
            {
                new Step {Type = typeof(SaveReceivedMessageStep).AssemblyQualifiedName},
                new Step {Type = typeof(DeterminePModesStep).AssemblyQualifiedName},
                new Step {Type = typeof(ValidateAS4MessageStep).AssemblyQualifiedName},
                new Step {Type = typeof(DecryptAS4MessageStep).AssemblyQualifiedName},
                new Step {Type = typeof(VerifySignatureAS4MessageStep).AssemblyQualifiedName},
                new Step {Type = typeof(DecompressAttachmentsStep).AssemblyQualifiedName},
                new Step {Type = typeof(UpdateReceivedAS4MessageBodyStep).AssemblyQualifiedName},
                new Step {Type = typeof(CreateAS4ReceiptStep).AssemblyQualifiedName},
                new Step {Type = typeof(SignAS4MessageStep).AssemblyQualifiedName},
                new Step {Type = typeof(SendAS4SignalMessageStep).AssemblyQualifiedName},
            };
        }

        private static Step[] CreateReceiveUnhappyFlowSteps()
        {
            return new[]
            {
                new Step {Type = typeof(CreateAS4ErrorStep).AssemblyQualifiedName},
                new Step {Type = typeof(SignAS4MessageStep).AssemblyQualifiedName},
                new Step {Type = typeof(SendAS4SignalMessageStep).AssemblyQualifiedName}
            };
        }
    }
}