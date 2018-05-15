using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.Strategies.Uploader;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Deliver
{
    /// <summary>
    /// Describes how the message payloads are uploaded to their respective media
    /// </summary>
    [Info("Upload attachments to deliver location")]
    [Description("This step uploads the message payloads to the destination that was configured in the receiving pmode.")]
    public class UploadAttachmentsStep : IStep
    {
        private readonly Func<DatastoreContext> _createDbContext;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IAttachmentUploaderProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadAttachmentsStep" /> class.
        /// </summary>
        public UploadAttachmentsStep() : this(Registry.Instance.AttachmentUploader) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadAttachmentsStep"/> class.
        /// Create a <see cref="IStep"/> implementation
        /// for uploading the AS4 Attachments to a configured location
        /// </summary>
        /// <param name="provider"></param>
        public UploadAttachmentsStep(IAttachmentUploaderProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadAttachmentsStep" /> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="createDbContext">Creates a database context.</param>
        public UploadAttachmentsStep(IAttachmentUploaderProvider provider, Func<DatastoreContext> createDbContext)
        {
            _provider = provider;
            _createDbContext = createDbContext;
        }

        /// <summary>
        /// Start uploading the AS4 Message Payloads
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            AS4Message as4Message = messagingContext.AS4Message;
            if (!as4Message.HasAttachments)
            {
                return StepResult.Success(messagingContext);
            }

            if (messagingContext.ReceivingPMode?.MessageHandling?.DeliverInformation == null)
            {
                throw new InvalidOperationException(
                    "Unable to send DeliverMessage: the ReceivingPMode does not contain any DeliverInformation");
            }

            IAttachmentUploader uploader = GetAttachmentUploader(messagingContext.ReceivingPMode);
            var results = new Collection<UploadResult>();

            foreach (UserMessage um in as4Message.UserMessages)
            {
                foreach (Attachment att in as4Message.Attachments.Where(a => a.MatchesAny(um.PayloadInfo)))
                {
                    UploadResult result = await TryUploadAttachmentAsync(att, um, uploader).ConfigureAwait(false);
                    results.Add(result);
                }
            }

            if (results.Any())
            {
                await UpdateDeliverMessageAccordinglyToUploadResult(
                    messageId: as4Message.GetPrimaryMessageId(),
                    result: results.Aggregate<DeliverResult>(DeliverResult.Reduce)); 
            }

            return await StepResult.SuccessAsync(messagingContext);
        }

        private IAttachmentUploader GetAttachmentUploader(ReceivingProcessingMode pmode)
        {
            Method payloadReferenceMethod = pmode.MessageHandling.DeliverInformation.PayloadReferenceMethod;
            if (payloadReferenceMethod.Type == null)
            {
                string description = $"Invalid configured Payload Reference Method in receive PMode {((IPMode) pmode).Id}";
                Logger.Error(description);

                throw new InvalidDataException(description);
            }

            IAttachmentUploader uploader = _provider.Get(payloadReferenceMethod.Type);
            uploader.Configure(payloadReferenceMethod);

            return uploader;
        }

        private static async Task<UploadResult> TryUploadAttachmentAsync(
            Attachment attachment, 
            UserMessage referringUserMessage, 
            IAttachmentUploader uploader)
        {
            try
            {
                Logger.Info($"{referringUserMessage.MessageId} Start Uploading Attachment...");

                UploadResult attachmentResult = 
                    await uploader.UploadAsync(attachment, referringUserMessage).ConfigureAwait(false);

                attachment.Location = attachmentResult.DownloadUrl;
                attachment.ResetContentPosition();

                return attachmentResult;
            }
            catch (Exception exception)
            {
                Logger.Error("Attachments cannot be uploaded");
                Logger.Error(exception.Message);

                throw;
            }
        }

        private async Task UpdateDeliverMessageAccordinglyToUploadResult(string messageId, DeliverResult result)
        {
            using (DatastoreContext context = _createDbContext())
            {
                var repository = new DatastoreRepository(context);
                var service = new RetryService(repository);

                service.UpdateDeliverMessageAccordinglyToDeliverResult(messageId, result);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}