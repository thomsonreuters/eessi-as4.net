using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Strategies.Retriever;
using Eu.EDelivery.AS4.Validators;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// Create an <see cref="AS4Message"/> from a <see cref="SubmitMessage"/>
    /// </summary>
    [Info("Create AS4 message for the submit message")]
    [Description("Create an AS4 Message for the submit message")]
    public class CreateAS4MessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private static readonly SubmitMessageValidator SubmitValidator = new SubmitMessageValidator();

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
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            AS4Message as4Message = CreateAS4MessageFromSubmit(messagingContext);
            await AssignAttachmentsForAS4Message(as4Message, messagingContext).ConfigureAwait(false);

            messagingContext.ModifyContext(as4Message);
            return StepResult.Success(messagingContext);
        }

        private static AS4Message CreateAS4MessageFromSubmit(MessagingContext messagingContext)
        {
            ValidateSubmitMessage(messagingContext.SubmitMessage);

            UserMessage userMessage = CreateUserMessage(messagingContext);
            Logger.Info($"{messagingContext} UserMessage with Id {userMessage.MessageId} created from Submit Message");

            return AS4Message.Create(userMessage, messagingContext.SendingPMode);
        }

        private static void ValidateSubmitMessage(SubmitMessage submitMessage)
        {
            SubmitValidator
                .Validate(submitMessage)
                .Result(
                    result => Logger.Trace($"Submit Message {submitMessage.MessageInfo.MessageId} is valid"),
                    result =>
                    {
                        result.LogErrors(Logger);
                        throw ThrowInvalidSubmitMessageException(submitMessage);
                        
                    });
        }

        private static InvalidMessageException ThrowInvalidSubmitMessageException(SubmitMessage submitMessage)
        {
            string description = $"(Submit) SubmitMessage {submitMessage.MessageInfo.MessageId} was invalid, see logging";
            Logger.Error(description);

            return new InvalidMessageException(description);
        }

        private static UserMessage CreateUserMessage(MessagingContext messagingContext)
        {
            Logger.Trace("Create UserMessage for SubmitMessage");
            return AS4Mapper.Map<UserMessage>(messagingContext.SubmitMessage);
        }

        private async Task AssignAttachmentsForAS4Message(AS4Message as4Message, MessagingContext context)
        {
            try
            {
                if (context.SubmitMessage.HasPayloads)
                {
                    Logger.Trace("Retrieve SubmitMessage payloads");

                    await as4Message.AddAttachments(
                        context.SubmitMessage.Payloads,
                        async payload => await RetrieveAttachmentContent(payload).ConfigureAwait(false)).ConfigureAwait(false);

                    Logger.Info($"{context.LogTag} Assigned {as4Message.Attachments.Count()} payloads to the AS4Message");
                }
                else
                {
                    Logger.Info($"{context.LogTag} SubmitMessage has no payloads to retrieve, so no will be added to the AS4Message");
                }
            }
            catch (Exception exception)
            {
                string description = $"{context.LogTag} Failed to retrieve SubmitMessage payloads";
                Logger.Error(description);
                Logger.Error($"{context} {exception.Message}");

                throw new ApplicationException(description, exception);
            }
        }

        private async Task<System.IO.Stream> RetrieveAttachmentContent(Payload payload)
        {
            return await _payloadProvider
                .Get(payload)
                .RetrievePayloadAsync(payload.Location)
                .ConfigureAwait(false);
        }
    }
}
