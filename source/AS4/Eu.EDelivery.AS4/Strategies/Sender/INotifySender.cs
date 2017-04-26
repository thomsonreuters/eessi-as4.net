using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    /// <summary>
    /// Interface to describe where the <see cref="NotifyMessageEnvelope"/> contents has to be send
    /// </summary>
    public interface INotifySender
    {
        /// <summary>
        /// Start sending the <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="notifyMessage"></param>
        Task SendAsync(NotifyMessageEnvelope notifyMessage);

        /// <summary>
        /// Configure the <see cref="INotifySender"/>
        /// with a given <paramref name="method"/>
        /// </summary>
        /// <param name="method"></param>
        void Configure(Method method);
    }
}
