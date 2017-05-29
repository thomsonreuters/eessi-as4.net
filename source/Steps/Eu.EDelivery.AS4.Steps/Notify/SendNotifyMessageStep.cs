using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Services;
using Eu.EDelivery.AS4.Strategies.Sender;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Notify
{
    /// <summary>
    /// Describes how a <see cref="NotifyMessage"/> is sent to the business application 
    /// </summary>
    public class SendNotifyMessageStep : IStep
    {
        private readonly INotifySenderProvider _provider;
        private readonly IOutMessageService _messageService;
        private readonly Func<DatastoreContext> _createContext;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private InternalMessage _internalMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendNotifyMessageStep"/> class
        /// </summary>
        public SendNotifyMessageStep()
            : this(Registry.Instance.NotifySenderProvider, Registry.Instance.CreateDatastoreContext) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SendNotifyMessageStep" /> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="createContext">The create context.</param>
        public SendNotifyMessageStep(INotifySenderProvider provider, Func<DatastoreContext> createContext)
        {
            _provider = provider;
            _createContext = createContext;
        }

        /// <summary>
        /// Start sending <see cref="NotifyMessage"/>
        /// to the consuming business application
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="AS4Exception">Throws exception when Notify Message is send incorrectly</exception>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            _internalMessage = internalMessage;
            Logger.Info($"{internalMessage.Prefix} Start sending Notify Message...");

            if (internalMessage.AS4Message.SendingPMode == null)
            {
                internalMessage.AS4Message.SendingPMode = RetrieveSendingPMode(internalMessage);
            }

            await TrySendNotifyMessage(internalMessage.NotifyMessage).ConfigureAwait(false);
            return await StepResult.SuccessAsync(internalMessage);
        }

        private SendingProcessingMode RetrieveSendingPMode(InternalMessage internalMessage)
        {
            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);
                string messageId = internalMessage.AS4Message.PrimarySignalMessage.RefToMessageId;

                return repository.FirstOrDefaultOutMessage(
                    messageId,
                    m => AS4XmlSerializer.FromString<SendingProcessingMode>(m.PMode));
            }
        }

        private async Task TrySendNotifyMessage(NotifyMessageEnvelope notifyMessage)
        {
            try
            {
                Method notifyMethod = GetNotifyMethod(notifyMessage);
                await SendNotifyMessage(notifyMessage, notifyMethod).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                throw ThrowAS4SendException("Notify Message was not send correctly", exception);
            }
        }

        private Method GetNotifyMethod(NotifyMessageEnvelope notifyMessage)
        {
            SendingProcessingMode sendPMode = _internalMessage.AS4Message.SendingPMode;
            ReceivingProcessingMode receivePMode = _internalMessage.AS4Message.ReceivingPMode;

            switch (notifyMessage.StatusCode)
            {
                case Status.Delivered: return sendPMode.ReceiptHandling.NotifyMethod;
                case Status.Error: return sendPMode.ErrorHandling.NotifyMethod;
                case Status.Exception: return DetermineMethod(sendPMode?.ExceptionHandling, receivePMode?.ExceptionHandling);
                default: throw new ArgumentOutOfRangeException($"Notify method not defined for status {notifyMessage.StatusCode}");
            }
        }

        private Method DetermineMethod(SendHandling sendHandling, Receivehandling receivehandling)
        {
            return IsNotifyMessageFormedBySending() ? sendHandling?.NotifyMethod : receivehandling?.NotifyMethod;
        }

        private async Task SendNotifyMessage(NotifyMessageEnvelope notifyMessage, Method notifyMethod)
        {
            INotifySender sender = _provider.GetNotifySender(notifyMethod.Type);
            sender.Configure(notifyMethod);
            await sender.SendAsync(notifyMessage).ConfigureAwait(false);
        }

        private AS4Exception ThrowAS4SendException(string description, Exception exception = null)
        {
            Logger.Error(description);

            AS4ExceptionBuilder builder = AS4ExceptionBuilder
                .WithDescription(description)
                .WithInnerException(exception)
                .WithMessageIds(_internalMessage.NotifyMessage.MessageInfo.MessageId)
                .WithErrorAlias(ErrorAlias.ConnectionFailure);

            AddPModeToBuilder(builder);

            return builder.Build();
        }

        private void AddPModeToBuilder(AS4ExceptionBuilder builder)
        {
            if (IsNotifyMessageFormedBySending())
            {
                builder.WithSendingPMode(_internalMessage.AS4Message.SendingPMode);
            }
            else
            {
                builder.WithReceivingPMode(_internalMessage.AS4Message.ReceivingPMode);
            }
        }

        public bool IsNotifyMessageFormedBySending()
        {
            return _internalMessage.AS4Message.SendingPMode?.Id != null;
        }
    }
}