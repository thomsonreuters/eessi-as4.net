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
            Name = name;
        }
    }
}