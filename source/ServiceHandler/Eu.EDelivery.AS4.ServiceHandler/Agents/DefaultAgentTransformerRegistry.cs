using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Transformers;

namespace Eu.EDelivery.AS4.ServiceHandler.Agents
{
    internal static class DefaultAgentTransformerRegistry
    {
        private static readonly IDictionary<AgentType, (Transformer, IEnumerable<Transformer>)> Registry = 
            new Dictionary<AgentType, (Transformer, IEnumerable<Transformer>)>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAgentTransformerRegistry"/> class.
        /// </summary>
        static DefaultAgentTransformerRegistry()
        {
            Registry[AgentType.ReceptionAwareness] = TransformerConfigEntry<ReceptionAwarenessTransformer>();
            Registry[AgentType.Deliver]            = TransformerConfigEntry<DeliverMessageTransformer>();
            Registry[AgentType.Submit]             = TransformerConfigEntry<SubmitMessageXmlTransformer>(typeof(SubmitPayloadTransformer));
            Registry[AgentType.OutboundProcessing] = TransformerConfigEntry<AS4MessageTransformer>();
            Registry[AgentType.PushSend]           = TransformerConfigEntry<OutMessageTransformer>();
            Registry[AgentType.PullSend]           = TransformerConfigEntry<AS4MessageTransformer>();
            Registry[AgentType.Receive]            = TransformerConfigEntry<ReceiveMessageTransformer>();
            Registry[AgentType.Notify]             = TransformerConfigEntry<NotifyMessageTransformer>();
            Registry[AgentType.Forward]            = TransformerConfigEntry<ForwardMessageTransformer>();
            Registry[AgentType.PullReceive]        = TransformerConfigEntry<PModeToPullRequestTransformer>();
        }

        private static (Transformer, IEnumerable<Transformer>) TransformerConfigEntry<TDefault>(params Type[] others)
        {
            return (TransformerConfig(typeof(TDefault)), others.Select(TransformerConfig));
        }

        private static Transformer TransformerConfig(Type t)
        {
            return new Transformer {Type = t.AssemblyQualifiedName};
        }

        internal static (Transformer defaultTransformer, IEnumerable<Transformer> otherTransformers) GetDefaultTransformerFor(AgentType agentType)
        {
            if (Registry.ContainsKey(agentType) == false)
            {
                throw new NotSupportedException($"There is no default Transformer available for agent-type {agentType}");
            }

            return Registry[agentType];
        }
    }
}