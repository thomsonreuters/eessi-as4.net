using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
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
        private readonly ILogger _logger;

        private InternalMessage _internalMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendNotifyMessageStep"/> class
        /// </summary>
        public SendNotifyMessageStep()
        {
            _provider = Registry.Instance.NotifySenderProvider;
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendNotifyMessageStep"/> class. 
        /// Create a <see cref="IStep"/> implementation
        /// to send a <see cref="NotifyMessage"/> 
        /// to the consuming business application
        /// </summary>
        /// <param name="provider">
        /// The provider.
        /// </param>
        public SendNotifyMessageStep(INotifySenderProvider provider)
        {
            _provider = provider;
            _logger = LogManager.GetCurrentClassLogger();
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
            _logger.Info($"{internalMessage.Prefix} Start sending Notify Message...");

            internalMessage.AS4Message.SendingPMode = RetrieveSendingPMode(internalMessage);

            await TrySendNotifyMessage(internalMessage.NotifyMessage).ConfigureAwait(false);
            return await StepResult.SuccessAsync(internalMessage);
        }

        private static SendingProcessingMode RetrieveSendingPMode(InternalMessage internalMessage)
        {
            using (var context = Registry.Instance.CreateDatastoreContext())
            {
                var repo = new DatastoreRepository(context);
                return repo.RetrieveSendingPModeForOutMessage(internalMessage.AS4Message.PrimarySignalMessage.RefToMessageId);
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
            _logger.Error(description);

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