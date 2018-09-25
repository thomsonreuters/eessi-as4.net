using System;
using System.ComponentModel;
using System.IO;
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
        private readonly IPayloadRetrieverProvider _payloadProvider;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private static readonly SubmitMessageValidator SubmitValidator = new SubmitMessageValidator();

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
            if (payloadPayloadProvider == null)
            {
                throw new ArgumentNullException(nameof(payloadPayloadProvider));
            }

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
            if (messagingContext == null)
            {
                throw new ArgumentNullException(nameof(messagingContext));
            }

            if (messagingContext.SubmitMessage == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CreateAS4MessageStep)} requires a SubmitMessage to create an AS4Message from but no AS4Message is present in the MessagingContext");
            }

            AS4Message as4Message = CreateAS4MessageFromSubmit(messagingContext);
            await AssignAttachmentsForAS4MessageAsync(as4Message, messagingContext).ConfigureAwait(false);

            messagingContext.ModifyContext(as4Message);
            return StepResult.Success(messagingContext);
        }

        private static AS4Message CreateAS4MessageFromSubmit(MessagingContext messagingContext)
        {
            ValidateSubmitMessage(messagingContext.SubmitMessage);

            Logger.Trace("Create UserMessage for SubmitMessage");
            var userMessage = AS4Mapper.Map<UserMessage>(messagingContext.SubmitMessage);
            Logger.Info($"{messagingContext.LogTag} UserMessage with Id \"{userMessage.MessageId}\" created from Submit Message");

            return AS4Message.Create(userMessage, messagingContext.SendingPMode);
        }

        private static void ValidateSubmitMessage(SubmitMessage submitMessage)
        {
            SubmitValidator
                .Validate(submitMessage)
                .Result(
                    result => Logger.Trace($"SubmitMessage \"{submitMessage.MessageInfo?.MessageId}\" is valid"),
                    result =>
                    {
                        result.LogErrors(Logger);

                        string description = $"SubmitMessage \"{submitMessage.MessageInfo?.MessageId}\" was invalid, see logging";
                        Logger.Error(description);

                        throw new InvalidMessageException(description);
                        
                    });
        }

        private async Task AssignAttachmentsForAS4MessageAsync(AS4Message as4Message, MessagingContext context)
        {
            try
            {
                if (context.SubmitMessage.HasPayloads)
                {
                    Logger.Trace("Retrieve SubmitMessage payloads");

                    await as4Message.AddAttachments(
                        context.SubmitMessage.Payloads,
                        async payload => await RetrieveAttachmentContentAsync(payload).ConfigureAwait(false)).ConfigureAwait(false);

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

        private async Task<Stream> RetrieveAttachmentContentAsync(Payload payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(
                    nameof(payload),
                    $@"Unable to retrieve {nameof(IPayloadRetriever)} for SubmitMessage Payload because it is 'null'");
            }

            IPayloadRetriever retriever = _payloadProvider.Get(payload);
            if (retriever == null)
            {
                throw new ArgumentNullException(
                    nameof(retriever), 
                    $@"No {nameof(IPayloadRetriever)} for Payload with Id: {payload.Id} can be found");
            }

            Task<Stream> retrievePayloadAsync = retriever.RetrievePayloadAsync(payload.Location);
            if (retrievePayloadAsync == null)
            {
                throw new ArgumentNullException(
                    nameof(retrievePayloadAsync),
                    $@"{nameof(IPayloadRetriever)} returns 'null' for Payload with Id: {payload.Id}");
            }
            
            return await retrievePayloadAsync.ConfigureAwait(false);
        }
    }
}
