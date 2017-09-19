using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Strategies.Retriever;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// Create an <see cref="AS4Message"/> from a <see cref="SubmitMessage"/>
    /// </summary>
    [Description("Create an AS4 Message for the submit message")]
    [Info("Create AS4 message")]
    public class CreateAS4MessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IPayloadRetrieverProvider _payloadProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAS4MessageStep"/> class.
        /// </summary>
        public CreateAS4MessageStep() : this(Registry.Instance.PayloadRetrieverProvider) { }

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
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            AS4Message as4Message = CreateAS4MessageFromSubmit(messagingContext);

            await RetrieveAttachmentsForAS4Message(as4Message, messagingContext);

            messagingContext.ModifyContext(as4Message);

            return StepResult.Success(messagingContext);
        }

        private static AS4Message CreateAS4MessageFromSubmit(MessagingContext messagingContext)
        {
            UserMessage userMessage = CreateUserMessage(messagingContext);
            Logger.Info($"UserMessage with Id {userMessage.MessageId} created from Submit Message");

            return AS4Message.Create(userMessage, messagingContext.SendingPMode);
        }

        private static UserMessage CreateUserMessage(MessagingContext messagingContext)
        {
            Logger.Debug($"Create UserMessage for Submit Message");

            return AS4Mapper.Map<UserMessage>(messagingContext.SubmitMessage);
        }

        private async Task RetrieveAttachmentsForAS4Message(AS4Message as4Message, MessagingContext context)
        {
            try
            {
                if (context.SubmitMessage.HasPayloads)
                {
                    Logger.Info($"{context.EbmsMessageId} Retrieve Submit Message Payloads");

                    await as4Message.AddAttachments(
                        context.SubmitMessage.Payloads,
                        async payload => await RetrieveAttachmentContent(payload));

                    Logger.Info($"{context.EbmsMessageId} Number of Payloads retrieved: {as4Message.Attachments.Count}");
                }
                else
                {
                    Logger.Info($"{context.EbmsMessageId} Submit Message has no Payloads to retrieve");
                }
            }
            catch (Exception exception)
            {
                throw FailedToRetrievePayloads(exception, context);
            }
        }

        private async Task<System.IO.Stream> RetrieveAttachmentContent(Payload payload)
        {
            return await _payloadProvider.Get(payload).RetrievePayloadAsync(payload.Location);
        }

        private static ApplicationException FailedToRetrievePayloads(Exception exception, MessagingContext messagingContext)
        {
            const string description = "Failed to retrieve Submit Message Payloads";
            Logger.Error(description);
            Logger.Error($"{messagingContext.EbmsMessageId} {exception.Message}");

            return new ApplicationException(description, exception);
        }
    }
}
