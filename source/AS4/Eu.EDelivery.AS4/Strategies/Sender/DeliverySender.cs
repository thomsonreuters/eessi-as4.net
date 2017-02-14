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

        public void Configure(Method method)
        {
            this.Method = method;
        }

        public void Send(DeliverMessageEnvelope deliverMessage)
        {
            Parameter locationParameter = this.Method["location"];

            var location = locationParameter?.Value;

            try
            {
                SendDeliverMessage(deliverMessage, location);
            }
            catch (Exception ex)
            {
                var description = $"Unable to send NotifyMessage to {location}: {ex.Message}";

                this.Log.Error(description);

                if (ex.InnerException != null)
                {
                    this.Log.Error(ex.InnerException.Message);
                }

                throw AS4ExceptionBuilder.WithDescription(description)
                    .WithInnerException(ex).Build();
            }
        }

        protected abstract void SendDeliverMessage(DeliverMessageEnvelope deliverMessage, string destinationUri);

    }
}