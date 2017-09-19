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
    [Info("Deliver attachments ipv upload attachments ?")]
    public class UploadAttachmentsStep : IStep
    {
        private readonly ILogger _logger;
        private readonly IAttachmentUploaderProvider _provider;

        private MessagingContext _messagingContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadAttachmentsStep"/> class.
        /// </summary>
        public UploadAttachmentsStep()
        {
            _provider = Registry.Instance.AttachmentUploader;
            _logger = LogManager.GetCurrentClassLogger();
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
            _logger = LogManager.GetCurrentClassLogger();
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

            var uploader = GetAttachmentUploader();

            await UploadAttachments(messagingContext.AS4Message.Attachments, uploader).ConfigureAwait(false);

            return await StepResult.SuccessAsync(messagingContext);
        }

        private IAttachmentUploader GetAttachmentUploader()
        {
            ReceivingProcessingMode pmode = _messagingContext.ReceivingPMode;
            Method payloadReferenceMethod = pmode.MessageHandling.DeliverInformation.PayloadReferenceMethod;
            PreConditionsPayloadReferenceMethod(pmode, payloadReferenceMethod);

            IAttachmentUploader uploader = _provider.Get(payloadReferenceMethod.Type);

            uploader.Configure(payloadReferenceMethod);

            return uploader;
        }

        private async Task UploadAttachments(IEnumerable<Attachment> attachments, IAttachmentUploader uploader)
        {
            await Task.WhenAll(attachments.Select(a => TryUploadAttachment(a, uploader)));
        }

        private async Task TryUploadAttachment(Attachment attachment, IAttachmentUploader uploader)
        {
            try
            {
                _logger.Info($"{_messagingContext.EbmsMessageId} Start Uploading Attachment...");
                await UploadAttachment(attachment, uploader).ConfigureAwait(false);

                attachment.ResetContentPosition();
            }
            catch (Exception exception)
            {
                const string description = "Attachments cannot be uploaded";
                _logger.Error(description);

                throw new ApplicationException(description, exception);
            }
        }

        private static async Task UploadAttachment(Attachment attachment, IAttachmentUploader uploader)
        {
            UploadResult attachmentResult = await uploader.UploadAsync(attachment).ConfigureAwait(false);

            attachment.Location = attachmentResult.DownloadUrl;
        }

        private void PreConditionsPayloadReferenceMethod(IPMode pmode, Method payloadReferenceMethod)
        {
            if (payloadReferenceMethod.Type == null)
            {
                string description = $"Invalid configured Payload Reference Method in receive PMode {pmode.Id}";
                _logger.Error(description);

                throw new InvalidDataException(description);
            }
        }
    }
}