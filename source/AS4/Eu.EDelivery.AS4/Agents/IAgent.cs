using System.Threading;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Agents
{
    /// <summary>
    /// Interface to provide an extendable Agent
    /// </summary>
    public interface IAgent
    {
        /// <summary>
        /// Gets the agent configuration.
        /// </summary>
        /// <value>The agent configuration.</value>
        AgentConfig AgentConfig { get; }
        
        /// <summary>
        /// Starts the specified agent.
        /// </summary>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        Task Start(CancellationToken cancellation);

        /// <summary>
        /// Stops this agent.
        /// </summary>
        void Stop();
    }
}