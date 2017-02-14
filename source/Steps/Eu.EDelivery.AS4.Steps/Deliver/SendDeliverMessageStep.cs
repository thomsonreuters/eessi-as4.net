using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Strategies.Sender;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Deliver
{
    /// <summary>
    /// Describes how a DeliverMessage is sent to the consuming business application. 
    /// </summary>
    public class SendDeliverMessageStep : IStep
    {
        private readonly IDeliverSenderProvider _provider;
        private readonly ILogger _logger;

        private InternalMessage _internalMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendDeliverMessageStep"/> class
        /// </summary>
        public SendDeliverMessageStep()
        {
            this._provider = Registry.Instance.DeliverSenderProvider;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendDeliverMessageStep"/> class
        /// Create a <see cref="IStep"/> implementation
        /// for sending the Deliver Message to the consuming business application
        /// </summary>
        /// <param name="provider"> The provider. </param>
        public SendDeliverMessageStep(IDeliverSenderProvider provider)
        {
            this._provider = provider;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start sending the AS4 Messages 
        /// to the consuming business application
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._internalMessage = internalMessage;
            this._logger.Info($"{internalMessage.Prefix} Start sending the Deliver Message " +
                              "to the consuming Business Application");

            TrySendDeliverMessage(internalMessage.DeliverMessage);
            return StepResult.SuccessAsync(internalMessage);
        }

        private void TrySendDeliverMessage(DeliverMessageEnvelope deliverMessage)
        {
            try
            {
                SendDeliverMessage(deliverMessage);
            }
            catch (Exception exception)
            {
                string description = $"{this._internalMessage.Prefix} Deliver Message was not send correctly";
                this._logger.Error(description);
                throw ThrowSendDeliverAS4Exception(description, exception);
            }
        }

        private void SendDeliverMessage(DeliverMessageEnvelope deliverMessage)
        {
            Method deliverMethod = this._internalMessage.AS4Message
                .ReceivingPMode.Deliver.DeliverMethod;

            IDeliverSender sender = this._provider.GetDeliverSender(deliverMethod.Type);
            sender.Configure(deliverMethod);
            sender.Send(deliverMessage);
        }

        private AS4Exception ThrowSendDeliverAS4Exception(string description, Exception innerException)
        {
            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithMessageIds(this._internalMessage.AS4Message.MessageIds)
                .WithExceptionType(ExceptionType.ConnectionFailure)
                .WithInnerException(innerException)
                .WithReceivingPMode(this._internalMessage.AS4Message.ReceivingPMode)
                .Build();
        }
    }
}