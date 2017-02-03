using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
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
            this._provider = Registry.Instance.NotifySenderProvider;
            this._logger = LogManager.GetCurrentClassLogger();
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
            this._provider = provider;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start sending <see cref="NotifyMessage"/>
        /// to the consuming business application
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="AS4Exception">Throws exception when Notify Message is send incorrectly</exception>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._internalMessage = internalMessage;
            this._logger.Info($"{internalMessage.Prefix} Start sending Notify Message...");

            TrySendNotifyMessage(internalMessage.NotifyMessage);
            return StepResult.SuccessAsync(internalMessage);
        }

        private void TrySendNotifyMessage(NotifyMessage notifyMessage)
        {
            try
            {
                Method notifyMethod = GetNotifyMethod(notifyMessage);
                PostNotifyMethodConditions(notifyMethod);
                SendNotifyMessage(notifyMessage, notifyMethod);
            }
            catch (Exception exception)
            {
                throw ThrowAS4SendException("Notify Message was not send correctly", exception);
            }
        }

        private void PostNotifyMethodConditions(Method notifyMethod)
        {
            if (notifyMethod != null) return;

            Status status = this._internalMessage.NotifyMessage.StatusInfo.Status;
            throw ThrowAS4SendException($"Notify Method not defined for Status: {status}");
        }

        private Method GetNotifyMethod(NotifyMessage notifyMessage)
        {
            SendingProcessingMode sendPMode = this._internalMessage.AS4Message.SendingPMode;
            ReceivingProcessingMode receivePMode = this._internalMessage.AS4Message.ReceivingPMode;

            switch (notifyMessage.StatusInfo.Status)
            {
                case Status.Delivered: return sendPMode.ReceiptHandling.NotifyMethod;
                case Status.Error: return sendPMode.ErrorHandling.NotifyMethod;
                case Status.Exception: return DetermineMethod(sendPMode?.ExceptionHandling, receivePMode?.ExceptionHandling);
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private Method DetermineMethod(SendHandling sendHandling, Receivehandling receivehandling)
        {
            return IsNotifyMessageFormedBySending() ? sendHandling?.NotifyMethod : receivehandling?.NotifyMethod;
        }

        private void SendNotifyMessage(NotifyMessage notifyMessage, Method notifyMethod)
        {
            INotifySender sender = this._provider.GetNotifySender(notifyMethod.Type);
            sender.Configure(notifyMethod);
            sender.Send(notifyMessage);
        }

        private AS4Exception ThrowAS4SendException(string description, Exception exception = null)
        {
            this._logger.Error(description);

            AS4ExceptionBuilder builder = AS4ExceptionBuilder
                .WithDescription(description)
                .WithInnerException(exception)                
                .WithMessageIds(this._internalMessage.NotifyMessage.MessageInfo.MessageId)
                .WithExceptionType(ExceptionType.ConnectionFailure);

            AddPModeToBuilder(builder);

            return builder.Build();
        }

        private void AddPModeToBuilder(AS4ExceptionBuilder builder)
        {
            if (IsNotifyMessageFormedBySending())
                builder.WithSendingPMode(this._internalMessage.AS4Message.SendingPMode);
            else
                builder.WithReceivingPMode(this._internalMessage.AS4Message.ReceivingPMode);
        }

        public bool IsNotifyMessageFormedBySending()
        {
            return this._internalMessage.AS4Message.SendingPMode?.Id != null;
        }
    }
}