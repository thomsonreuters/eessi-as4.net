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
        private static readonly IDictionary<AgentType, TransformerConfigEntry> Registry = 
            new Dictionary<AgentType, TransformerConfigEntry>();

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

        private static TransformerConfigEntry TransformerConfigEntry<TDefault>(params Type[] others)
        {
            return new TransformerConfigEntry(TransformerConfig(typeof(TDefault)), others.Select(TransformerConfig));
        }

        private static Transformer TransformerConfig(Type t)
        {
            return new Transformer {Type = t.AssemblyQualifiedName};
        }

        internal static TransformerConfigEntry GetDefaultTransformerFor(AgentType agentType)
        {
            if (Registry.ContainsKey(agentType) == false)
            {
                throw new NotSupportedException($"There is no default Transformer available for agent-type {agentType}");
            }

            return Registry[agentType];
        }
    }

    /// <summary>
    /// Transformer Configuration Entry to wrap the information about the different <see cref="ITransformer"/> implementations that can be used for an <see cref="AgentType"/>.
    /// </summary>
    public class TransformerConfigEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransformerConfigEntry" /> class.
        /// </summary>
        /// <param name="defaultTransformer">The default transformer.</param>
        /// <param name="otherTransformers">The other transformers.</param>
        public TransformerConfigEntry(Transformer defaultTransformer, IEnumerable<Transformer> otherTransformers)
        {
            DefaultTransformer = defaultTransformer;
            OtherTransformers = otherTransformers;
        }

        /// <summary>
        /// Gets the configuration to create the default <see cref="ITransformer"/> for an <see cref="AgentType"/>.
        /// </summary>
        /// <value>The default transformer.</value>
        public Transformer DefaultTransformer { get; }

        /// <summary>
        /// Gets the list of configurations to create other <see cref="ITransformer"/> implementations for an <see cref="AgentType"/>.
        /// </summary>
        /// <value>The other transformers.</value>
        public IEnumerable<Transformer> OtherTransformers { get; }
    }
}