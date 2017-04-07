using System;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    public abstract class DeliverySender : IDeliverSender
    {
        protected Logger Log { get; } = LogManager.GetCurrentClassLogger();

        protected Method Method { get; private set; }

        /// <summary>
        /// Configure the <see cref="IDeliverSender"/>
        /// with a given <paramref name="method"/>
        /// </summary>
        /// <param name="method"></param>
        public void Configure(Method method)
        {
            Method = method;
        }

        /// <summary>
        /// Start sending the <see cref="DeliverMessage"/>
        /// </summary>
        /// <param name="deliverMessage"></param>
        public void Send(DeliverMessageEnvelope deliverMessage)
        {
            Parameter locationParameter = Method["location"];

            string location = locationParameter?.Value;

            try
            {
                SendDeliverMessage(deliverMessage, location);
            }
            catch (Exception ex)
            {
                string description = $"Unable to send NotifyMessage to {location}: {ex.Message}";

                Log.Error(description);

                if (ex.InnerException != null)
                {
                    Log.Error(ex.InnerException.Message);
                }

                throw AS4ExceptionBuilder.WithDescription(description).WithInnerException(ex).Build();
            }
        }

        /// <summary>
        /// Send a given <paramref name="deliverMessage"/> to a specified <paramref name="destinationUri"/>.
        /// </summary>
        /// <param name="deliverMessage">The message.</param>
        /// <param name="destinationUri">The uri.</param>
        protected abstract void SendDeliverMessage(DeliverMessageEnvelope deliverMessage, string destinationUri);
    }
}