using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private InternalMessage _internalMessage;

        /// <summary>
        /// Iniitializes a new instance of the <see cref="UploadAttachmentsStep"/> class
        /// </summary>
        public UploadAttachmentsStep()
        {
            _provider = Registry.Instance.AttachmentUploader;
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadAttachmentsStep"/> class
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
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            if (!internalMessage.AS4Message.HasAttachments)
            {
                return await StepResult.SuccessAsync(internalMessage);
            }

            _internalMessage = internalMessage;

            await UploadAttachments(internalMessage.AS4Message.Attachments);
            return await StepResult.SuccessAsync(internalMessage);
        }

        private async Task UploadAttachments(IEnumerable<Attachment> attachments)
        {
            await Task.WhenAll(attachments.Select(TryUploadAttachment));
        }

        private async Task TryUploadAttachment(Attachment attachment)
        {
            try
            {
                _logger.Info($"{_internalMessage.Prefix} Start Uploading Attachment...");
                await UploadAttachment(attachment);
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
            UploadResult attachmentResult = await uploader.UploadAsync(attachment);

            attachment.Location = attachmentResult.DownloadUrl;
        }

        private Method GetPayloadReferenceMethod()
        {
            ReceivingProcessingMode pmode = _internalMessage.AS4Message.ReceivingPMode;
            Method payloadReferenceMethod = pmode.Deliver.PayloadReferenceMethod;
            PreConditionsPayloadReferenceMethod(pmode, payloadReferenceMethod);

            return payloadReferenceMethod;
        }

        private void PreConditionsPayloadReferenceMethod(ReceivingProcessingMode pmode, Method payloadReferenceMethod)
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
                .WithReceivingPMode(_internalMessage.AS4Message.ReceivingPMode);

            if (_internalMessage.DeliverMessage != null)
            {
                builder.WithMessageIds(_internalMessage.DeliverMessage.MessageInfo.MessageId);
            }
            else if (_internalMessage.AS4Message?.PrimaryUserMessage != null)
            {
                builder.WithMessageIds(_internalMessage.AS4Message.PrimaryUserMessage.MessageId);
            }

            return builder.Build();
        }
    }
}