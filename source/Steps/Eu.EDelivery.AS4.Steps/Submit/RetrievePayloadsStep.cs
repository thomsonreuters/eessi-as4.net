using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Strategies.Retriever;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// Add the Payloads specified inside the <see cref="SubmitMessage" />
    /// to the <see cref="AS4Message" /> as Attachments
    /// </summary>
    public class RetrievePayloadsStep : IStep
    {
        private readonly IPayloadRetrieverProvider _provider;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetrievePayloadsStep"/> class
        /// </summary>
        public RetrievePayloadsStep()
        {
            this._provider = Registry.Instance.PayloadRetrieverProvider;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetrievePayloadsStep"/> class
        /// Create Retrieve Payloads Step with a given Payload Strategy Provider
        /// </summary>
        /// <param name="provider">
        /// </param>
        public RetrievePayloadsStep(IPayloadRetrieverProvider provider)
        {
            this._provider = provider;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Execute to retrieve the Payloads from the <see cref="SubmitMessage" />
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            _logger.Info($"{internalMessage.Prefix} Executing RetrievePayloadsStep");

            if (!internalMessage.SubmitMessage.HasPayloads)
                return ReturnSameInternalMessage(internalMessage);

            TryRetrievePayloads(internalMessage);
            return StepResult.SuccessAsync(internalMessage);
        }

        private Task<StepResult> ReturnSameInternalMessage(InternalMessage internalMessage)
        {
            this._logger.Info($"{internalMessage.Prefix} Submit Message has no Payloads to retrieve");
            return StepResult.SuccessAsync(internalMessage);
        }

        private void TryRetrievePayloads(InternalMessage internalMessage)
        {
            try
            {
                this._logger.Info($"{internalMessage.Prefix} Retrieve Submit Message Payloads");
                internalMessage.AddAttachments(payload => this._provider.Get(payload).RetrievePayload(payload.Location));
                this._logger.Info($"{internalMessage.Prefix} Number of Payloads retrieved: {internalMessage.AS4Message.Attachments.Count}");
            }
            catch (Exception exception)
            {
                throw ThrowAS4FailedRetrievePayloadsException(internalMessage, exception);
            }
        }

        private AS4Exception ThrowAS4FailedRetrievePayloadsException(InternalMessage internalMessage, Exception exception)
        {
            string description = $"{internalMessage.Prefix} Failed to retrieve Submit Message Payloads";
            this._logger.Error(description);
            this._logger.Error($"{internalMessage.Prefix} {exception.Message}");

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithInnerException(exception)
                .WithMessageIds(internalMessage.AS4Message.MessageIds)
                .WithSendingPMode(internalMessage.AS4Message.SendingPMode)
                .Build();
        }
    }
}