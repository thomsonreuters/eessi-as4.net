using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Decompress the incoming Payloads
    /// </summary>
    public class DecompressAttachmentsStep : IStep
    {
        private const string GzipContentType = "application/gzip";

        private readonly ILogger _logger;
        private AS4Message _as4Message;
        private InternalMessage _internalMessage;

        /// <summary>
        /// Initializes a new instance of the type <see cref="DecompressAttachmentsStep"/> class
        /// </summary>
        public DecompressAttachmentsStep()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Decompress any Attachments
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="AS4Exception">Throws exception when AS4 Message cannot be decompressed</exception>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._internalMessage = internalMessage;
            this._as4Message = internalMessage.AS4Message;

            if (!this._as4Message.HasAttachments)
                return ReturnSameInternalMessage(internalMessage);

            await TryDecompressAttachments();
            return StepResult.Success(internalMessage);
        }

        private StepResult ReturnSameInternalMessage(InternalMessage internalMessage)
        {
            this._logger.Debug($"{this._internalMessage.Prefix} AS4Message hasn't got any Attachments");
            return StepResult.Success(internalMessage);
        }

        private async Task TryDecompressAttachments()
        {
            try
            {
                await DecompressAttachments();
                this._logger.Info(
                    $"{this._internalMessage.Prefix} Try Decompress AS4 Message Attachments with GZip Compression");
            }
            catch (Exception exception)
            {
                throw ThrowAS4CannotDecompressException(exception);
            }
        }

        private async Task DecompressAttachments()
        {
            foreach (Attachment attachment in this._as4Message.Attachments)
            {
                if (IsAttachmentNotCompressed(attachment)) continue;
                if (HasntAttachmentMimeTypePartProperty(attachment))
                    throw ThrowMissingMimeTypePartyPropertyException(attachment);

                await DecompressAttachment(attachment);
                AssignAttachmentProperties(attachment);
            }
        }

        private AS4Exception ThrowMissingMimeTypePartyPropertyException(Attachment attachment)
        {
            string description = $"Attachment {attachment.Id} hasn't got a MimeType PartProperty";
            return new AS4ExceptionBuilder().WithDescription(description).Build();
        }

        private bool IsAttachmentNotCompressed(Attachment attachment)
        {
            bool isNotCompressed = !attachment.ContentType.Equals(GzipContentType) &&
                                   !attachment.Properties.ContainsKey("CompressionType");

            if (isNotCompressed)
                this._logger.Debug($"{this._internalMessage.Prefix} Attachment {attachment.Id} is not Compressed");

            return isNotCompressed;
        }

        private bool HasntAttachmentMimeTypePartProperty(Attachment attachment)
        {
            return !attachment.Properties.ContainsKey("MimeType");
        }

        private async Task DecompressAttachment(Attachment attachment)
        {
            this._logger.Debug($"{this._internalMessage.Prefix} Attachment {attachment.Id} will be Decompressed");

            attachment.Content.Position = 0;
            var memoryStream = new MemoryStream();

            using (var gzipCompression = new GZipStream(
                attachment.Content, CompressionMode.Decompress, leaveOpen: true))
            {
                await gzipCompression.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                attachment.Content = memoryStream;
            }
        }

        private void AssignAttachmentProperties(Attachment attachment)
        {
            attachment.Properties["CompressionType"] = GzipContentType;
            string mimeType = GetMimeType(attachment);
            attachment.Properties["MimeType"] = mimeType;
            attachment.ContentType = mimeType;
        }

        private string GetMimeType(Attachment attachment)
        {
            List<PartInfo> partInfos = this._as4Message.PrimaryUserMessage.PayloadInfo;
            return partInfos.Find(i => i.Href.Equals("cid:" + attachment.Id)).Properties["MimeType"];
        }

        private AS4Exception ThrowAS4CannotDecompressException(Exception exception)
        {
            const string description = "Cannot decompress the message";
            this._logger.Error(description);

            return new AS4ExceptionBuilder()
                .WithDescription(description)
                .WithMessageIds(this._as4Message.MessageIds)
                .WithErrorCode(ErrorCode.Ebms0303)
                .WithInnerException(exception)
                .WithReceivingPMode(this._as4Message.ReceivingPMode)
                .Build();
        }
    }
}