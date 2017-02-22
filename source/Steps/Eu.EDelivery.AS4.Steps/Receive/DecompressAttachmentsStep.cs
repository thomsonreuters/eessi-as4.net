using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;
using Exception = System.Exception;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Decompress the incoming Payloads
    /// </summary>
    public class DecompressAttachmentsStep : IStep
    {
        private const string GzipContentType = "application/gzip";

        private readonly ILogger _logger;
        
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

            if (!internalMessage.AS4Message.HasAttachments)
            {
                return ReturnSameInternalMessage(internalMessage);
            }

            await TryDecompressAttachments(internalMessage.AS4Message);
            return StepResult.Success(internalMessage);
        }

        private StepResult ReturnSameInternalMessage(InternalMessage internalMessage)
        {
            this._logger.Debug($"{this._internalMessage.Prefix} AS4Message hasn't got any Attachments");
            return StepResult.Success(internalMessage);
        }

        private async Task TryDecompressAttachments(AS4Message as4Message)
        {
            try
            {
                await DecompressAttachments(as4Message.PrimaryUserMessage.PayloadInfo.ToList(), as4Message.Attachments);
                this._logger.Info(
                    $"{this._internalMessage.Prefix} Try Decompress AS4 Message Attachments with GZip Compression");
            }
            catch (Exception exception)
            {
                throw ThrowAS4CannotDecompressException(as4Message, exception);
            }
        }

        private async Task DecompressAttachments(List<Model.Core.PartInfo> messagePayloadInformation, ICollection<Attachment> attachments)
        {            
            foreach (Attachment attachment in attachments)
            {
                if (IsAttachmentNotCompressed(attachment))
                {
                    continue;
                }

                if (HasntAttachmentMimeTypePartProperty(attachment))
                {
                    throw ThrowMissingMimeTypePartyPropertyException(attachment);
                }

                await DecompressAttachment(attachment);
                AssignAttachmentProperties(messagePayloadInformation, attachment);
            }
        }

        private static AS4Exception ThrowMissingMimeTypePartyPropertyException(Attachment attachment)
        {
            string description = $"Attachment {attachment.Id} hasn't got a MimeType PartProperty";
            return AS4ExceptionBuilder.WithDescription(description).Build();
        }

        private bool IsAttachmentNotCompressed(Attachment attachment)
        {
            bool isNotCompressed = !attachment.ContentType.Equals(GzipContentType, StringComparison.OrdinalIgnoreCase) &&
                                   !attachment.Properties.ContainsKey("CompressionType");

            if (isNotCompressed)
            {
                this._logger.Debug($"{this._internalMessage.Prefix} Attachment {attachment.Id} is not Compressed");
            }

            return isNotCompressed;
        }

        private static bool HasntAttachmentMimeTypePartProperty(Attachment attachment)
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

        private void AssignAttachmentProperties(List<Model.Core.PartInfo> messagePayloadInfo, Attachment attachment)
        {
            attachment.Properties["CompressionType"] = GzipContentType;
            string mimeType = GetMimeType(messagePayloadInfo, attachment);
            attachment.Properties["MimeType"] = mimeType;
            attachment.ContentType = mimeType;
        }

        private string GetMimeType(List<Model.Core.PartInfo> messagePayloadInfo, Attachment attachment)
        {
            return messagePayloadInfo.Find(i => i.Href.Equals("cid:" + attachment.Id)).Properties["MimeType"];
        }

        private AS4Exception ThrowAS4CannotDecompressException( AS4Message as4Message, Exception exception)
        {
            const string description = "Cannot decompress the message";
            this._logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithMessageIds(as4Message.MessageIds)
                .WithErrorCode(ErrorCode.Ebms0303)
                .WithInnerException(exception)
                .WithReceivingPMode(as4Message.ReceivingPMode)
                .Build();
        }
    }
}