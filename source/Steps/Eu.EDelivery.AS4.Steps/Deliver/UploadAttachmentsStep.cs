using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Strategies.Uploader;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Deliver
{
    /// <summary>
    /// Describes how the message payloads are uploaded to their respective media
    /// </summary>
    [Description("This step uploads the message payloads to the destination that was configured in the receiving pmode.")]
    [Info("Upload attachments to deliver location")]
    public class UploadAttachmentsStep : IStep
    {
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
        /// Start uploading the AS4 Message Payloads
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
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

            foreach (UserMessage um in as4Message.UserMessages)
            {
                await Task.WhenAll(as4Message.Attachments
                    .Where(a => a.MatchesAny(um.PayloadInfo))
                    .Select(p => TryUploadAttachment(p, um, uploader)));
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

        private static async Task TryUploadAttachment(Attachment attachment, UserMessage referringUserMessage, IAttachmentUploader uploader)
        {
            try
            {
                Logger.Info($"{referringUserMessage.MessageId} Start Uploading Attachment...");

                UploadResult attachmentResult = 
                    await uploader.UploadAsync(attachment, referringUserMessage).ConfigureAwait(false);

                attachment.Location = attachmentResult.DownloadUrl;
                attachment.ResetContentPosition();
            }
            catch (Exception exception)
            {
                Logger.Error("Attachments cannot be uploaded");
                Logger.Error(exception.Message);

                throw;
            }
        }
    }
}