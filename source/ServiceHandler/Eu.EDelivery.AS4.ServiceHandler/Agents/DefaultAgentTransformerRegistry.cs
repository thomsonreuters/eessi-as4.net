using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Transformers;

namespace Eu.EDelivery.AS4.ServiceHandler.Agents
{
    internal static class DefaultAgentTransformerRegistry
    {
        private static readonly IDictionary<AgentType, Transformer> TransformerRegistry =
            new Dictionary<AgentType, Transformer>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAgentTransformerRegistry"/> class.
        /// </summary>
        static DefaultAgentTransformerRegistry()
        {
            TransformerRegistry.Add(AgentType.ReceptionAwareness, new Transformer { Type = typeof(ReceptionAwarenessTransformer).AssemblyQualifiedName });
            TransformerRegistry.Add(AgentType.Deliver, new Transformer { Type = typeof(DeliverMessageTransformer).AssemblyQualifiedName });
            TransformerRegistry.Add(AgentType.Submit, new Transformer { Type = typeof(SubmitMessageXmlTransformer).AssemblyQualifiedName });
            TransformerRegistry.Add(AgentType.OutboundProcessing, new Transformer { Type = typeof(AS4MessageTransformer).AssemblyQualifiedName });
            TransformerRegistry.Add(AgentType.PushSend, new Transformer { Type = typeof(OutMessageTransformer).AssemblyQualifiedName });
            TransformerRegistry.Add(AgentType.PullSend, new Transformer { Type = typeof(AS4MessageTransformer).AssemblyQualifiedName });
            TransformerRegistry.Add(AgentType.Receive, new Transformer { Type = typeof(ReceiveMessageTransformer).AssemblyQualifiedName });
            TransformerRegistry.Add(AgentType.Notify, new Transformer { Type = typeof(NotifyMessageTransformer).AssemblyQualifiedName });
            TransformerRegistry.Add(AgentType.Deliver, new Transformer { Type = typeof(DeliverMessageTransformer).AssemblyQualifiedName });
            TransformerRegistry.Add(AgentType.Forward, new Transformer { Type = typeof(ForwardMessageTransformer).AssemblyQualifiedName });
        }

        internal static Transformer GetDefaultTransformerFor(AgentType agentType)
        {
            if (TransformerRegistry.ContainsKey(agentType) == false)
            {
                throw new NotSupportedException($"There is no default Transformer available for agent-type {agentType}");
            }

            return TransformerRegistry[agentType];
        }
    }
}