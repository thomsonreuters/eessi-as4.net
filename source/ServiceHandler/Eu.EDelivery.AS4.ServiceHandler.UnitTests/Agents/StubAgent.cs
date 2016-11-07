using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Model;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Transformers;

namespace Eu.EDelivery.AS4.ServiceHandler.UnitTests.Agents
{
    /// <summary>
    /// Stub for the abstract <see cref="Agent" />
    /// </summary>
    public class StubAgent : Agent
    {
        public StubAgent(IReceiver receiver, IStep step, ITransformer transformer = null)
            : base(receiver, transformer, step) { }

        /// <summary>
        /// Perform action when Message is received
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        public override Task<InternalMessage> OnReceived(ReceivedMessage message, CancellationToken cancellationToken)
        {
            return null;
        }
    }
}