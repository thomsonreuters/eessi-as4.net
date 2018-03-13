using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    /// <summary>
    /// Decorator to add the 'reliable' functionality of the sending functionality 
    /// to both the <see cref="IDeliverSender"/> and <see cref="INotifySender"/> implementation.
    /// </summary>
    [NotConfigurable]
    internal class ReliableSender : IDeliverSender, INotifySender
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDeliverSender _deliverSender;
        private readonly INotifySender _notifySender;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableSender"/> class.
        /// </summary>
        /// <param name="deliverSender"></param>
        public ReliableSender(IDeliverSender deliverSender)
        {
            _deliverSender = deliverSender;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableSender"/> class.
        /// </summary>
        /// <param name="notifySender"></param>
        public ReliableSender(INotifySender notifySender)
        {
            _notifySender = notifySender;
        }

        /// <summary>
        /// Configure the <see cref="INotifySender"/>
        /// with a given <paramref name="method"/>
        /// </summary>
        /// <param name="method"></param>
        public void Configure(Method method)
        {
            _deliverSender?.Configure(method);
            _notifySender?.Configure(method);
        }

        /// <summary>
        /// Start sending the <see cref="DeliverMessage"/>
        /// </summary>
        /// <param name="deliverMessage"></param>
        public async Task SendAsync(DeliverMessageEnvelope deliverMessage)
        {
            await SendMessage(deliverMessage, _deliverSender.SendAsync).ConfigureAwait(false);
        }

        /// <summary>
        /// Start sending the <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="notifyMessage"></param>
        public async Task SendAsync(NotifyMessageEnvelope notifyMessage)
        {
            await SendMessage(notifyMessage, _notifySender.SendAsync).ConfigureAwait(false);
        }

        private static async Task SendMessage<T>(T message, Func<T, Task> sendAction)
        {
            try
            {
                await sendAction(message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string description = $"Unable to send {message.GetType().Name} to the configured endpoint: {ex.Message}";

                Logger.Error(description);

                if (ex.InnerException != null)
                {
                    Logger.Error(ex.InnerException.Message);
                }

                throw;
            }
        }
    }
}
