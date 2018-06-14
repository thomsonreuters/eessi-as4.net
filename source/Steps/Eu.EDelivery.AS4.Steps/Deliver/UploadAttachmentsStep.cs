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
    [Description("This step uploads the deliver message payloads to the destination that was configured in the receiving pmode.")]
    public class UploadAttachmentsStep : IStep
    {
        private readonly Func<DatastoreContext> _createDbContext;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IAttachmentUploaderProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadAttachmentsStep" /> class.
        /// </summary>
        public UploadAttachmentsStep() 
            : this(Registry.Instance.AttachmentUploader, Registry.Instance.CreateDatastoreContext) { }

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
                    "Unable to send the deliver message: the ReceivingPMode does not contain any <DeliverInformation />." + 
                    "Please provide a correct <DeliverInformation /> tag to indicate where the deliver message (and its attachments) should be send to.");
            }

            IAttachmentUploader uploader = GetAttachmentUploader(messagingContext.ReceivingPMode);
            var results = new Collection<UploadResult>();

            foreach (UserMessage um in as4Message.UserMessages)
            {
                foreach (Attachment att in as4Message.Attachments.Where(a => a.MatchesAny(um.PayloadInfo)))
                {
                    UploadResult result = await TryUploadAttachmentAsync(att, um, uploader).ConfigureAwait(false);
                    if (result.Status == SendResult.Success)
                    {
                        Logger.Info($"{messagingContext.LogTag} Attachment '{att.Id}' is delivered at: {att.Location}");
                    }

                    results.Add(result);
                }
            }

            SendResult accResult = results
                .Select(r => r.Status)
                .Aggregate(SendResultUtils.Reduce);

            await UpdateDeliverMessageAccordinglyToUploadResult(
                messageId: as4Message.GetPrimaryMessageId(),
                status: accResult);

            if (accResult == SendResult.Success)
            {
                return StepResult.Success(messagingContext);
            }
            
            return StepResult.Failed(messagingContext);
        }

        private IAttachmentUploader GetAttachmentUploader(ReceivingProcessingMode pmode)
        {
            Method payloadReferenceMethod = pmode.MessageHandling.DeliverInformation.PayloadReferenceMethod;
            if (payloadReferenceMethod.Type == null)
            {
                string description = $"(Deliver) Invalid configured Payload Reference Method in receive PMode {((IPMode) pmode).Id}";
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
                Logger.Trace($"(Deliver)[{referringUserMessage.MessageId}] Start Uploading Attachment...");

                UploadResult attachmentResult = 
                    await uploader.UploadAsync(attachment, referringUserMessage).ConfigureAwait(false);

                attachment.Location = attachmentResult.DownloadUrl;
                attachment.ResetContentPosition();

                Logger.Trace($"(Deliver)[{referringUserMessage.MessageId}] Attachment uploaded succesfully");
                return attachmentResult;
            }
            catch (Exception exception)
            {
                Logger.Error(
                    $"(Deliver) Attachment {attachment.Id} cannot be uploaded "
                    + $"because of an exception: {Environment.NewLine}" + exception);

                return UploadResult.FatalFail;
            }
        }

        private async Task UpdateDeliverMessageAccordinglyToUploadResult(string messageId, SendResult status)
        {
            using (DatastoreContext context = _createDbContext())
            {
                var repository = new DatastoreRepository(context);
                var service = new MarkForRetryService(repository);

                service.UpdateDeliverMessageForUploadResult(messageId, status);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}