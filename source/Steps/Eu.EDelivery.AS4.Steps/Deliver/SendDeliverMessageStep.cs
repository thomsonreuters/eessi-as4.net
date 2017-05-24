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
            _provider = Registry.Instance.DeliverSenderProvider;
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendDeliverMessageStep"/> class
        /// Create a <see cref="IStep"/> implementation
        /// for sending the Deliver Message to the consuming business application
        /// </summary>
        /// <param name="provider"> The provider. </param>
        public SendDeliverMessageStep(IDeliverSenderProvider provider)
        {
            _provider = provider;
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start sending the AS4 Messages 
        /// to the consuming business application
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            _internalMessage = internalMessage;
            _logger.Info($"{internalMessage.Prefix} Start sending the Deliver Message " +
                              "to the consuming Business Application");

            await TrySendDeliverMessage(internalMessage.DeliverMessage).ConfigureAwait(false);
            return await StepResult.SuccessAsync(internalMessage);
        }

        private async Task TrySendDeliverMessage(DeliverMessageEnvelope deliverMessage)
        {
            try
            {
                await SendDeliverMessage(deliverMessage).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                string description = $"{_internalMessage.Prefix} Deliver Message was not send correctly";
                _logger.Error(description);
                throw ThrowSendDeliverAS4Exception(description, exception);
            }
        }

        private async Task SendDeliverMessage(DeliverMessageEnvelope deliverMessage)
        {
            Method deliverMethod = _internalMessage.AS4Message?.ReceivingPMode.Deliver.DeliverMethod;

            IDeliverSender sender = _provider.GetDeliverSender(deliverMethod?.Type);
            sender.Configure(deliverMethod);
            await sender.SendAsync(deliverMessage).ConfigureAwait(false);
        }

        private AS4Exception ThrowSendDeliverAS4Exception(string description, Exception innerException)
        {
            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithMessageIds(_internalMessage.AS4Message?.MessageIds ?? new string[0])
                .WithErrorAlias(ErrorAlias.ConnectionFailure)
                .WithInnerException(innerException)
                .WithReceivingPMode(_internalMessage.AS4Message?.ReceivingPMode)
                .Build();
        }
    }
}