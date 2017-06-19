using System.Threading;
using System.Threading.Tasks;

using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Agents
{
    /// <summary>
    /// Interface to provide an extendable Agent
    /// </summary>
    public interface IAgent
    {
        AgentConfig AgentConfig { get; }     
        Task Start(CancellationToken cancellation);
        void Stop();
    }
}