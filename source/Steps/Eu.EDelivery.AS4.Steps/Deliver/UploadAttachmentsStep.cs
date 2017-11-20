using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
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

        private MessagingContext _messagingContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadAttachmentsStep"/> class.
        /// </summary>
        public UploadAttachmentsStep() : this(Registry.Instance.AttachmentUploader)
        {
        }

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
            if (!messagingContext.AS4Message.HasAttachments)
            {
                return await StepResult.SuccessAsync(messagingContext);
            }

            _messagingContext = messagingContext;

            if (_messagingContext.ReceivingPMode?.MessageHandling?.DeliverInformation == null)
            {
                throw new InvalidOperationException("Unable to send DeliverMessage: the ReceivingPMode does not contain any DeliverInformation");
            }

            var uploader = GetAttachmentUploader(_messagingContext.ReceivingPMode);

            // Retrieve and upload all payloads per user-message.
            foreach (var um in _messagingContext.AS4Message.UserMessages)
            {
                var payloads = _messagingContext.AS4Message.Attachments.Where(a => a.MatchesAny(um.PayloadInfo));

                await UploadAttachments(payloads, um, uploader).ConfigureAwait(false);
            }

            return await StepResult.SuccessAsync(messagingContext);
        }

        private IAttachmentUploader GetAttachmentUploader(ReceivingProcessingMode pmode)
        {
            Method payloadReferenceMethod = pmode.MessageHandling.DeliverInformation.PayloadReferenceMethod;
            PreConditionsPayloadReferenceMethod(pmode, payloadReferenceMethod);

            IAttachmentUploader uploader = _provider.Get(payloadReferenceMethod.Type);

            uploader.Configure(payloadReferenceMethod);

            return uploader;
        }

        private static async Task UploadAttachments(IEnumerable<Attachment> attachments, UserMessage referringUserMessage, IAttachmentUploader uploader)
        {
            foreach (var attachment in attachments)
            {
                await TryUploadAttachment(attachment, referringUserMessage, uploader);
            }
        }

        private static async Task TryUploadAttachment(Attachment attachment, UserMessage referringUserMessage, IAttachmentUploader uploader)
        {
            try
            {
                Logger.Info($"{referringUserMessage.MessageId} Start Uploading Attachment...");

                await UploadAttachment(attachment, referringUserMessage, uploader).ConfigureAwait(false);

                attachment.ResetContentPosition();
            }
            catch (Exception exception)
            {
                Logger.Error("Attachments cannot be uploaded");
                Logger.Error(exception.Message);

                throw;
            }
        }

        private static async Task UploadAttachment(Attachment attachment, UserMessage belongsToUserMessage, IAttachmentUploader uploader)
        {
            UploadResult attachmentResult = await uploader.UploadAsync(attachment, belongsToUserMessage).ConfigureAwait(false);

            attachment.Location = attachmentResult.DownloadUrl;
        }

        private static void PreConditionsPayloadReferenceMethod(IPMode pmode, Method payloadReferenceMethod)
        {
            if (payloadReferenceMethod.Type == null)
            {
                string description = $"Invalid configured Payload Reference Method in receive PMode {pmode.Id}";
                Logger.Error(description);

                throw new InvalidDataException(description);
            }
        }
    }
}