namespace Eu.EDelivery.AS4.Agents
{
    /// <summary>
    /// Agent Configuration
    /// </summary>
    public class AgentConfig
    {
        public string Name { get; }

        public AgentConfig(string name)
        {
            this.Name = name;
        }
    }

    /// <summary>
    /// Null Object for the Agent Config
    /// </summary>
    public class NullAgentConfig : AgentConfig
    {
        private NullAgentConfig() : base("[Null Agent Config]")
        {            
        }

        public static readonly NullAgentConfig Default = new NullAgentConfig();
    }
}