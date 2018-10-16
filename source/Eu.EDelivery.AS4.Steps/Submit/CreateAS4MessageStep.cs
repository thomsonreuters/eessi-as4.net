using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            if (messagingContext.SendingPMode == null)
            {
                Logger.Debug("No SendingPMode was found, only use information from SubmitMessage to create AS4 UserMessage");
            }

            SubmitMessage submitMessage = messagingContext.SubmitMessage;
            ValidateSubmitMessage(submitMessage);

            Logger.Trace("Create UserMessage for SubmitMessage");
            var userMessage = AS4Mapper.Map<UserMessage>(submitMessage);

            Logger.Info($"{messagingContext.LogTag} UserMessage with Id \"{userMessage.MessageId}\" created from Submit Message");
            AS4Message as4Message = AS4Message.Create(userMessage, messagingContext.SendingPMode);

            IEnumerable<Attachment> attachments = 
                await RetrieveAttachmentsForAS4MessageAsync(submitMessage.Payloads)
                    .ConfigureAwait(false);

            as4Message.AddAttachments(attachments);

            messagingContext.ModifyContext(as4Message);
            return StepResult.Success(messagingContext);
        }

        private static void ValidateSubmitMessage(SubmitMessage submitMessage)
        {
            SubmitValidator
                .Validate(submitMessage)
                .Result(
                    result => Logger.Trace($"SubmitMessage \"{submitMessage.MessageInfo.MessageId}\" is valid"),
                    result =>
                    {
                        result.LogErrors(Logger);
                        string description = $"SubmitMessage \"{submitMessage.MessageInfo.MessageId}\" was invalid, see logging";

                        Logger.Error(description);
                        throw new InvalidMessageException(description);
                        
                    });
        }

        private async Task<IEnumerable<Attachment>> RetrieveAttachmentsForAS4MessageAsync(IEnumerable<Payload> payloads)
        {
            if (!payloads.Any())
            {
                Logger.Debug("SubmitMessage has no payloads to retrieve, so no will be added to the AS4Message");
                return Enumerable.Empty<Attachment>();
            }

            try
            {
                Logger.Trace("Start retrieving SubmitMessage payloads contents...");

                var attachments = new Collection<Attachment>();
                foreach (Payload payload in payloads)
                {
                    Stream content =
                        await _payloadProvider
                              .Get(payload)
                              .RetrievePayloadAsync(payload.Location)
                              .ConfigureAwait(false);

                    attachments.Add(new Attachment(payload.Id, content, payload.MimeType));
                }

                Logger.Trace($"Successfully retrieved {attachments.Count()} payloads");
                return attachments.AsEnumerable();
            }
            catch (Exception exception)
            {
                const string description = "(Submit) Failed to retrieve SubmitMessage payloads";
                Logger.Error(description);
                Logger.Error(exception);

                throw new ApplicationException(description, exception);
            }
        }
    }
}
