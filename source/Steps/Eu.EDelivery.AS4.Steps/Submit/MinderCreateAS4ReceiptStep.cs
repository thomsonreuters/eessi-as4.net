using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Core;
using NLog;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// <see cref="IStep"/> implementation to create a Minder AS4 Error
    /// </summary>
    public class MinderCreateAS4ReceiptStep : IStep
    {
        private InternalMessage _internalMessage;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MinderCreateAS4ReceiptStep"/>
        /// </summary>
        public MinderCreateAS4ReceiptStep()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start creating Minder Receipt message
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._internalMessage = internalMessage;
            return CreateEmptySoapResult();
        }

        private Task<StepResult> CreateEmptySoapResult()
        {
            this._logger.Info($"{this._internalMessage.Prefix} Empty SOAP Envelope will be send to requested party");
            var emptyInternalMessage = new InternalMessage(CreateEmptyAS4Message());

            return StepResult.SuccessAsync(emptyInternalMessage);
        }

        private AS4Message CreateEmptyAS4Message()
        {
            SendingProcessingMode sendPMode = this._internalMessage.AS4Message.SendingPMode;

            return new AS4MessageBuilder().WithSendingPMode(sendPMode).Build();
        }
    }
}