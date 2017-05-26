using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Strategies.Uploader;
using Eu.EDelivery.AS4.Streaming;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Deliver
{
    /// <summary>
    /// Describes how the message payloads are uploaded to their respective media
    /// </summary>
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

            await UploadAttachments(messagingContext.AS4Message.Attachments).ConfigureAwait(false);

            return await StepResult.SuccessAsync(messagingContext);
        }

        private async Task UploadAttachments(IEnumerable<Attachment> attachments)
        {
            await Task.WhenAll(attachments.Select(TryUploadAttachment));
        }

        private async Task TryUploadAttachment(Attachment attachment)
        {
            try
            {
                _logger.Info($"{_messagingContext.Prefix} Start Uploading Attachment...");
                await UploadAttachment(attachment).ConfigureAwait(false);

                attachment.ResetContentPosition();
            }
            catch (Exception exception)
            {
                throw ThrowUploadAS4Exception("Attachments cannot be uploaded", exception);
            }
        }

        private async Task UploadAttachment(Attachment attachment)
        {
            Method payloadReferenceMethod = GetPayloadReferenceMethod();

            IAttachmentUploader uploader = _provider.Get(payloadReferenceMethod.Type);
            uploader.Configure(payloadReferenceMethod);
            UploadResult attachmentResult = await uploader.UploadAsync(attachment).ConfigureAwait(false);

            attachment.Location = attachmentResult.DownloadUrl;
        }

        private Method GetPayloadReferenceMethod()
        {
            ReceivingProcessingMode pmode = _messagingContext.ReceivingPMode;
            Method payloadReferenceMethod = pmode.Deliver.PayloadReferenceMethod;
            PreConditionsPayloadReferenceMethod(pmode, payloadReferenceMethod);

            return payloadReferenceMethod;
        }

        private void PreConditionsPayloadReferenceMethod(IPMode pmode, Method payloadReferenceMethod)
        {
            if (payloadReferenceMethod.Type == null)
            {
                string description = $"Invalid configured Payload Reference Method in receive PMode {pmode.Id}";
                _logger.Error(description);

                throw AS4ExceptionBuilder.WithDescription(description).Build();
            }
        }

        private AS4Exception ThrowUploadAS4Exception(string description, Exception exception = null)
        {
            _logger.Error(description);

            AS4ExceptionBuilder builder = AS4ExceptionBuilder
                .WithDescription(description)
                .WithInnerException(exception)
                .WithReceivingPMode(_messagingContext.ReceivingPMode);

            if (_messagingContext.DeliverMessage != null)
            {
                builder.WithMessageIds(_messagingContext.DeliverMessage.MessageInfo.MessageId);
            }
            else if (_messagingContext.AS4Message?.PrimaryUserMessage != null)
            {
                builder.WithMessageIds(_messagingContext.AS4Message.PrimaryUserMessage.MessageId);
            }

            return builder.Build();
        }
    }
}