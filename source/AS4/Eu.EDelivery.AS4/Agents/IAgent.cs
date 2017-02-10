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
        AgentConfig AgentConfig { get; set; }
        Task<InternalMessage> OnReceived(ReceivedMessage message, CancellationToken cancellationToken);
        Task Start(CancellationToken cancellationToken);
        void Stop();
    }
}