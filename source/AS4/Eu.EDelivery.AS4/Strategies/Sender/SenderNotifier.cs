using System;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    /// <summary>
    /// SenderNotifiers are responsible for sending a NotificationMessage to a specific endpoint.
    /// </summary>
    public abstract class SenderNotifier : INotifySender
    {
        protected Logger Log { get; } = LogManager.GetCurrentClassLogger();
        
        protected Method Method { get; private set; }

        public void Configure(Method method)
        {
            // TODO: the formatter that should be used should be defined here. (Output as xml, json, etc...)

            this.Method = method;
        }

        public void Send(NotifyMessageEnvelope notifyMessage)
        {
            Parameter locationParameter = this.Method["location"];

            var location = locationParameter?.Value;

            try
            {
                SendNotifyMessage(notifyMessage, location);
            }
            catch (Exception ex)
            {
                var description = $"Unable to send NotifyMessage to {location}: {ex.Message}";

                Log.Error(description);

                if (ex.InnerException != null)
                {
                    Log.Error(ex.InnerException.Message);
                }

                throw AS4ExceptionBuilder.WithDescription(description)
                                         .WithInnerException(ex).Build();                
            }            
        }

        protected abstract void SendNotifyMessage(NotifyMessageEnvelope notifyMessage, string destinationUri);


    }
}
