using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Strategies.Sender;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Sender
{
    /// <summary>
    /// <see cref="IDeliverSender"/>, <see cref="INotifySender"/> implementation to sabotage the sending.
    /// </summary>
    public class SaboteurSender : IDeliverSender, INotifySender
    {
        /// <summary>
        /// Configure the <see cref="IDeliverSender" />
        /// with a given <paramref name="method" />
        /// </summary>
        /// <param name="method"></param>
        public void Configure(AS4.Model.PMode.Method method) {}

        /// <summary>
        /// Start sending the <see cref="DeliverMessage" />
        /// </summary>
        /// <param name="deliverMessage"></param>
        public Task SendAsync(DeliverMessageEnvelope deliverMessage)
        {
            throw new SaboteurException("Sabotage 'Deliver' Send");
        }

        /// <summary>
        /// Start sending the <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="notifyMessage"></param>
        public Task SendAsync(NotifyMessageEnvelope notifyMessage)
        {
            throw new SaboteurException("Sabotage 'Notify' Send");
        }
    }

    public class SaboteurException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SaboteurException" /> class.
        /// </summary>
        /// <param name="message">Exception Message</param>
        public SaboteurException(string message) : base(message) { }
    }
}