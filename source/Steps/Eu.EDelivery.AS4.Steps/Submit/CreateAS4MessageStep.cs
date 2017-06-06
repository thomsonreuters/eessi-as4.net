using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Strategies.Retriever;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// Create an <see cref="AS4Message"/> from a <see cref="SubmitMessage"/>
    /// </summary>
    public class CreateAS4MessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IPayloadRetrieverProvider _payloadProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAS4MessageStep"/> class.
        /// </summary>
        public CreateAS4MessageStep() : this(Registry.Instance.PayloadRetrieverProvider) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAS4MessageStep" /> class.
        /// </summary>
        /// <param name="payloadPayloadProvider">The payload provider.</param>
        public CreateAS4MessageStep(IPayloadRetrieverProvider payloadPayloadProvider)
        {
            _payloadProvider = payloadPayloadProvider;
        }

        /// <summary>
        /// Start Mapping from a <see cref="SubmitMessage"/> 
        /// to an <see cref="AS4Message"/>
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="AS4Exception">Thrown when creating an <see cref="AS4Message"/> Fails (Mapping, Building...)</exception>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            AS4Message as4Message = CreateAS4Message(messagingContext);

            await RetrieveAttachmentsForAS4Message(as4Message, messagingContext);

            return StepResult.Success(messagingContext.CloneWith(as4Message));
        }

        private static AS4Message CreateAS4Message(MessagingContext messagingContext)
        {
            try
            {
                return CreateAS4MessageFromSubmit(messagingContext);
            }
            catch (Exception exception)
            {
                throw UnableToCreateAS4Message(exception, messagingContext);
            }
        }

        private static AS4Exception UnableToCreateAS4Message(Exception innerException, MessagingContext messagingContext)
        {
            string generatedMessageId = IdentifierFactory.Instance.Create();
            string description = $"[generated: {generatedMessageId}] Unable to Create AS4 Message from Submit Message";
            Logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithSendingPMode(messagingContext?.SendingPMode)
                .WithMessageIds(generatedMessageId)
                .WithInnerException(innerException)
                .Build();
        }

        private static AS4Message CreateAS4MessageFromSubmit(MessagingContext messagingContext)
        {
            UserMessage userMessage = CreateUserMessage(messagingContext);
            Logger.Info($"[{userMessage.MessageId}] Create AS4Message with Submit Message");

            return new AS4MessageBuilder()
                .WithUserMessage(userMessage)
                .Build();
        }

        private static UserMessage CreateUserMessage(MessagingContext messagingContext)
        {
            Logger.Debug("Map Submit Message to UserMessage");
            
            return AS4Mapper.Map<UserMessage>(messagingContext.SubmitMessage);
        }

        private async Task RetrieveAttachmentsForAS4Message(AS4Message as4Message, MessagingContext context)
        {
            try
            {
                if (context.SubmitMessage.HasPayloads)
                {
                    Logger.Info($"{context.Prefix} Retrieve Submit Message Payloads");

                    await as4Message.AddAttachments(
                        context.SubmitMessage.Payloads,
                        async payload => await RetrieveAttachmentContent(payload));

                    Logger.Info($"{context.Prefix} Number of Payloads retrieved: {as4Message.Attachments.Count}");
                }
                else
                {
                    Logger.Info($"{context.Prefix} Submit Message has no Payloads to retrieve");
                }
            }
            catch (Exception exception)
            {
                throw FailedToRetrievePayloads(exception, context, as4Message);
            }
        }

        private async Task<System.IO.Stream> RetrieveAttachmentContent(Payload payload)
        {
            return await _payloadProvider.Get(payload).RetrievePayloadAsync(payload.Location);
        }

        private static AS4Exception FailedToRetrievePayloads(Exception exception, MessagingContext messagingContext, AS4Message as4Message)
        {
            string description = $"{messagingContext.Prefix} Failed to retrieve Submit Message Payloads";
            Logger.Error(description);
            Logger.Error($"{messagingContext.Prefix} {exception.Message}");

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithInnerException(exception)
                .WithMessageIds(as4Message.MessageIds)
                .WithSendingPMode(messagingContext.SendingPMode)
                .Build();
        }
    }
}
