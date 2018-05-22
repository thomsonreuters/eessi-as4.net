using System;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Agents
{
    /// <summary>
    /// Agent Configuration
    /// </summary>
    public class AgentConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentConfig"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public AgentConfig(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public AgentType Type { get; internal set; }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>The settings.</value>
        public AgentSettings Settings { get; internal set; }
    }

    public enum AgentType
    {
        Submit,
        Receive,
        PushSend,
        Deliver,
        Notify,        
        ReceptionAwareness,
        PullReceive,
        PullSend,
        OutboundProcessing,
        Forward
    }
}