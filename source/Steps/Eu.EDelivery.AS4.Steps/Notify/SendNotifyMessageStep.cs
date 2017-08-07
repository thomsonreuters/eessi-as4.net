using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Strategies.Sender;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Notify
{
    /// <summary>
    /// Describes how a <see cref="NotifyMessage"/> is sent to the business application 
    /// </summary>
    public class SendNotifyMessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly INotifySenderProvider _provider;
        private readonly Func<DatastoreContext> _createContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendNotifyMessageStep"/> class
        /// </summary>
        public SendNotifyMessageStep()
            : this(Registry.Instance.NotifySenderProvider, Registry.Instance.CreateDatastoreContext) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SendNotifyMessageStep" /> class.
        /// Create a <see cref="IStep" /> implementation
        /// to send a <see cref="NotifyMessage" />
        /// to the consuming business application
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="dataContextRetriever">The data context retriever.</param>
        public SendNotifyMessageStep(INotifySenderProvider provider, Func<DatastoreContext> dataContextRetriever)
        {
            _provider = provider;
            _createContext = dataContextRetriever;
        }

        /// <summary>
        /// Start sending <see cref="NotifyMessage"/>
        /// to the consuming business application
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            Logger.Info($"{messagingContext.Prefix} Start sending Notify Message...");

            if (messagingContext.SendingPMode == null)
            {
                SendingProcessingMode pmode = RetrieveSendingPMode(messagingContext);
                if (pmode != null)
                {
                    messagingContext.SendingPMode = pmode;
                }
            }

            await SendNotifyMessage(messagingContext).ConfigureAwait(false);
            return await StepResult.SuccessAsync(messagingContext);
        }

        private SendingProcessingMode RetrieveSendingPMode(MessagingContext messagingContext)
        {
            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);
                string messageId = messagingContext.NotifyMessage.MessageInfo.RefToMessageId;

                return repository.GetOutMessageData(
                    messageId,
                    m => AS4XmlSerializer.FromString<SendingProcessingMode>(m.PMode));
            }
        }

        private async Task SendNotifyMessage(MessagingContext message)
        {
            NotifyMessageEnvelope notifyMessage = message.NotifyMessage;
            Method notifyMethod = GetNotifyMethod(message);

            INotifySender sender = _provider.GetNotifySender(notifyMethod.Type);
            sender.Configure(notifyMethod);

            await sender.SendAsync(notifyMessage).ConfigureAwait(false);
        }

        private static Method GetNotifyMethod(MessagingContext messagingContext)
        {
            NotifyMessageEnvelope notifyMessage = messagingContext.NotifyMessage;
            SendingProcessingMode sendPMode = messagingContext.SendingPMode;
            ReceivingProcessingMode receivePMode = messagingContext.ReceivingPMode;

            switch (notifyMessage.StatusCode)
            {
                case Status.Delivered: return sendPMode.ReceiptHandling.NotifyMethod;
                case Status.Error: return sendPMode.ErrorHandling.NotifyMethod;
                case Status.Exception: return DetermineMethod(sendPMode, sendPMode?.ExceptionHandling, receivePMode?.ExceptionHandling);
                default: throw new ArgumentOutOfRangeException($"Notify method not defined for status {notifyMessage.StatusCode}");
            }
        }

        private static Method DetermineMethod(IPMode sendPMode, SendHandling sendHandling, ReceiveHandling receiveHandling)
        {
            return IsNotifyMessageFormedBySending(sendPMode) ? sendHandling?.NotifyMethod : receiveHandling?.NotifyMethod;
        }

        private static bool IsNotifyMessageFormedBySending(IPMode pmode)
        {
            return pmode?.Id != null;
        }
    }
}