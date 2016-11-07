namespace Eu.EDelivery.AS4.Agents
{
    /// <summary>
    /// Agent Configuration
    /// </summary>
    public class AgentConfig
    {
        public string Name { get; set; }

        public AgentConfig() {}

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
        public NullAgentConfig()
        {
            this.Name = "[Null Agent Config]";
        }
    }
}