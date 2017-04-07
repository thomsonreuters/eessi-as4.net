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
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IPayloadRetrieverProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetrievePayloadsStep" /> class
        /// </summary>
        public RetrievePayloadsStep()
        {
            _provider = Registry.Instance.PayloadRetrieverProvider;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetrievePayloadsStep" /> class
        /// Create Retrieve Payloads Step with a given Payload Strategy Provider
        /// </summary>
        /// <param name="provider">
        /// </param>
        public RetrievePayloadsStep(IPayloadRetrieverProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Execute to retrieve the Payloads from the <see cref="SubmitMessage" />
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            Logger.Info($"{internalMessage.Prefix} Executing RetrievePayloadsStep");

            if (!internalMessage.SubmitMessage.HasPayloads) return await ReturnSameInternalMessage(internalMessage);

            await TryRetrievePayloads(internalMessage);
            return await StepResult.SuccessAsync(internalMessage);
        }

        private Task<StepResult> ReturnSameInternalMessage(InternalMessage internalMessage)
        {
            Logger.Info($"{internalMessage.Prefix} Submit Message has no Payloads to retrieve");
            return StepResult.SuccessAsync(internalMessage);
        }

        private async Task TryRetrievePayloads(InternalMessage internalMessage)
        {
            try
            {
                Logger.Info($"{internalMessage.Prefix} Retrieve Submit Message Payloads");

                await internalMessage.AddAttachments(
                    async payload => await _provider.Get(payload).RetrievePayloadAsync(payload.Location));

                Logger.Info($"{internalMessage.Prefix} Number of Payloads retrieved: {internalMessage.AS4Message.Attachments.Count}");
            }
            catch (Exception exception)
            {
                throw ThrowAS4FailedRetrievePayloadsException(internalMessage, exception);
            }
        }

        private AS4Exception ThrowAS4FailedRetrievePayloadsException(InternalMessage internalMessage, Exception exception)
        {
            string description = $"{internalMessage.Prefix} Failed to retrieve Submit Message Payloads";
            Logger.Error(description);
            Logger.Error($"{internalMessage.Prefix} {exception.Message}");

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithInnerException(exception)
                .WithMessageIds(internalMessage.AS4Message.MessageIds)
                .WithSendingPMode(internalMessage.AS4Message.SendingPMode)
                .Build();
        }
    }
}