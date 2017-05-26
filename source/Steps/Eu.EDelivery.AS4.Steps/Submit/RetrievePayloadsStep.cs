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
        public RetrievePayloadsStep() : this(Registry.Instance.PayloadRetrieverProvider) {}

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
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            Logger.Info($"{messagingContext.Prefix} Executing RetrievePayloadsStep");

            if (!messagingContext.SubmitMessage.HasPayloads)
            {
                Logger.Info($"{messagingContext.Prefix} Submit Message has no Payloads to retrieve");
                return await StepResult.SuccessAsync(messagingContext);
            }

            await TryRetrievePayloads(messagingContext).ConfigureAwait(false);

            return await StepResult.SuccessAsync(messagingContext);
        }
        
        private async Task TryRetrievePayloads(MessagingContext messagingContext)
        {
            try
            {
                Logger.Info($"{messagingContext.Prefix} Retrieve Submit Message Payloads");

                await messagingContext.AddAttachments(
                    async payload => await _provider.Get(payload).RetrievePayloadAsync(payload.Location));

                Logger.Info($"{messagingContext.Prefix} Number of Payloads retrieved: {messagingContext.AS4Message.Attachments.Count}");
            }
            catch (Exception exception)
            {
                throw ThrowAS4FailedRetrievePayloadsException(messagingContext, exception);
            }
        }

        private static AS4Exception ThrowAS4FailedRetrievePayloadsException(MessagingContext messagingContext, Exception exception)
        {
            string description = $"{messagingContext.Prefix} Failed to retrieve Submit Message Payloads";
            Logger.Error(description);
            Logger.Error($"{messagingContext.Prefix} {exception.Message}");

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithInnerException(exception)
                .WithMessageIds(messagingContext.AS4Message.MessageIds)
                .WithSendingPMode(messagingContext.SendingPMode)
                .Build();
        }
    }
}