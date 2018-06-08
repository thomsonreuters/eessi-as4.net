using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;

namespace Eu.EDelivery.AS4.Agents
{
    public class RetryAgent : IAgent
    {
        private readonly IReceiver _receiver;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryAgent"/> class.
        /// </summary>
        public RetryAgent(IReceiver receiver)
        {
            _receiver = receiver;
        }

        /// <summary>
        /// Gets the agent configuration.
        /// </summary>
        /// <value>The agent configuration.</value>
        public AgentConfig AgentConfig { get; } = new AgentConfig("Retry Agent");

        /// <summary>
        /// Starts the specified agent.
        /// </summary>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        public async Task Start(CancellationToken cancellation)
        {
            await Task.Factory.StartNew(
                () => _receiver.StartReceiving(OnReceived, cancellation),
                TaskCreationOptions.LongRunning);
        }

        private Task<MessagingContext> OnReceived(ReceivedMessage arg1, CancellationToken arg2)
        {
            
        }

        private static Operation GetOperationFrom(ReceivedMessage rm)
        {
            if (rm is ReceivedEntityMessage rem)
            {
                
            }
        }

        /// <summary>
        /// Stops this agent.
        /// </summary>
        public void Stop()
        {
            _receiver.StopReceiving();
        }
    }
}
