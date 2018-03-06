using System;
using System.ComponentModel;
using System.Linq;
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
    [Description("Send a notification message using the method that is configured in the PMode")]
    [Info("Send notification message")]
    public class SendNotifyMessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly INotifySenderProvider _provider;
        private readonly Func<DatastoreContext> _createContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendNotifyMessageStep"/> class
        /// </summary>
        public SendNotifyMessageStep()
            : this(Registry.Instance.NotifySenderProvider, Registry.Instance.CreateDatastoreContext) { }

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
            if (messagingContext.SendingPMode == null)
            {
                SendingProcessingMode pmode =
                    RetrieveSendingPModeForMessageWithEbmsMessageId(messagingContext.NotifyMessage
                                                                                    .MessageInfo
                                                                                    .RefToMessageId);

                if (pmode != null)
                {
                    messagingContext.SendingPMode = pmode;
                }
            }

            Logger.Info($"{messagingContext.EbmsMessageId} Start sending Notify Message...");
            await SendNotifyMessage(messagingContext).ConfigureAwait(false);

            Logger.Info($"{messagingContext.EbmsMessageId} Notify Message sent");
            return StepResult.Success(messagingContext);
        }

        private SendingProcessingMode RetrieveSendingPModeForMessageWithEbmsMessageId(string ebmsMessageId)
        {
            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);

                var outMessageData = 
                    repository.GetOutMessageData(where: m => m.EbmsMessageId == ebmsMessageId && m.Intermediary == false,
                                                selection: m => new { m.PMode, m.ModificationTime })
                              .OrderByDescending(m => m.ModificationTime)
                              .FirstOrDefault();

                if (outMessageData == null)
                {
                    return null;
                }

                return AS4XmlSerializer.FromString<SendingProcessingMode>(outMessageData.PMode);
            }
        }

        private async Task SendNotifyMessage(MessagingContext messagingContext)
        {
            Method notifyMethod = GetNotifyMethod(messagingContext);

            INotifySender sender = _provider.GetNotifySender(notifyMethod.Type);
            sender.Configure(notifyMethod);

            await sender.SendAsync(messagingContext.NotifyMessage).ConfigureAwait(false);
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
            bool isNotifyMessageFormedBySending = sendPMode?.Id != null;

            return isNotifyMessageFormedBySending
                ? sendHandling?.NotifyMethod
                : receiveHandling?.NotifyMethod;
        }
    }
}