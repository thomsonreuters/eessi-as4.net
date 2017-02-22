using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
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
            this._provider = Registry.Instance.AttachmentUploader;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadAttachmentsStep"/> class
        /// Create a <see cref="IStep"/> implementation
        /// for uploading the AS4 Attachments to a configured location
        /// </summary>
        /// <param name="provider"></param>
        public UploadAttachmentsStep(IAttachmentUploaderProvider provider)
        {
            this._provider = provider;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start uploading the AS4 Message Payloads
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            if (!internalMessage.AS4Message.HasAttachments)
                return StepResult.SuccessAsync(internalMessage);

            this._internalMessage = internalMessage;

            UploadAttachments(internalMessage.AS4Message.Attachments);
            return StepResult.SuccessAsync(internalMessage);
        }

        private void UploadAttachments(IEnumerable<Attachment> attachments)
        {
            foreach (Attachment attachment in attachments)
                TryUploadAttachment(attachment);
        }

        private void TryUploadAttachment(Attachment attachment)
        {
            try
            {
                this._logger.Info($"{this._internalMessage.Prefix} Start Uploading Attachment...");
                UploadAttachment(attachment);
            }
            catch (Exception exception)
            {
                throw ThrowUploadAS4Exception("Attachments cannot be uploaded", exception);
            }
        }

        private void UploadAttachment(Attachment attachment)
        {
            Method payloadReferenceMethod = GetPayloadReferenceMethod();

            IAttachmentUploader uploader = this._provider.Get(payloadReferenceMethod.Type);
            uploader.Configure(payloadReferenceMethod);
            uploader.Upload(attachment);
        }

        private Method GetPayloadReferenceMethod()
        {
            ReceivingProcessingMode pmode = this._internalMessage.AS4Message.ReceivingPMode;
            Method payloadReferenceMethod = pmode.Deliver.PayloadReferenceMethod;
            PreConditionsPayloadReferenceMethod(pmode, payloadReferenceMethod);

            return payloadReferenceMethod;
        }

        private void PreConditionsPayloadReferenceMethod(ReceivingProcessingMode pmode, Method payloadReferenceMethod)
        {
            if (payloadReferenceMethod.Type != null) return;

            string description = $"Invalid configured Payload Reference Method in receive PMode {pmode.Id}";
            this._logger.Error(description);
            throw AS4ExceptionBuilder.WithDescription(description).Build();
        }

        private AS4Exception ThrowUploadAS4Exception(string description, Exception exception = null)
        {
            this._logger.Error(description);

            var builder = AS4ExceptionBuilder
                .WithDescription(description)
                .WithInnerException(exception)
                .WithReceivingPMode(this._internalMessage.AS4Message.ReceivingPMode);

            if (_internalMessage.DeliverMessage != null)
            {
                builder.WithMessageIds(this._internalMessage.DeliverMessage.MessageInfo.MessageId);
            }

            return builder.Build();
        }
    }
}