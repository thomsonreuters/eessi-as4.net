using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    /// <summary>
    /// Interface to describe where the <see cref="DeliverMessage"/> has to be send
    /// </summary>
    public interface IDeliverSender
    {
        /// <summary>
        /// Configure the <see cref="IDeliverSender"/>
        /// with a given <paramref name="method"/>
        /// </summary>
        /// <param name="method"></param>
        void Configure(Method method);

        /// <summary>
        /// Start sending the <see cref="DeliverMessage"/>
        /// </summary>
        /// <param name="deliverMessage"></param>
        Task<DeliverMessageResult> SendAsync(DeliverMessageEnvelope deliverMessage);
    }
}
