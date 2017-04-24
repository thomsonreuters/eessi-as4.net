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
using Eu.EDelivery.AS4.Streaming;
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
            _logger = LogManager.GetCurrentClassLogger();
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
            _internalMessage = internalMessage;

            if (!internalMessage.AS4Message.HasAttachments)
            {
                return await ReturnSameInternalMessage(internalMessage);
            }

            await TryDecompressAttachments(internalMessage.AS4Message);
            return await StepResult.SuccessAsync(internalMessage);
        }

        private async Task<StepResult> ReturnSameInternalMessage(InternalMessage internalMessage)
        {
            _logger.Debug($"{_internalMessage.Prefix} AS4Message hasn't got any Attachments");
            return await StepResult.SuccessAsync(internalMessage);
        }

        private async Task TryDecompressAttachments(AS4Message as4Message)
        {
            try
            {
                await DecompressAttachments(as4Message.PrimaryUserMessage.PayloadInfo.ToList(), as4Message.Attachments);
                _logger.Info(
                    $"{_internalMessage.Prefix} Try Decompress AS4 Message Attachments with GZip Compression");
            }
            catch (Exception exception)
            {
                throw ThrowAS4CannotDecompressException(as4Message, exception);
            }
        }

        private async Task DecompressAttachments(List<PartInfo> messagePayloadInformation, ICollection<Attachment> attachments)
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
                _logger.Debug($"{_internalMessage.Prefix} Attachment {attachment.Id} is not Compressed");
            }

            return isNotCompressed;
        }

        private static bool HasntAttachmentMimeTypePartProperty(Attachment attachment)
        {
            return !attachment.Properties.ContainsKey("MimeType");
        }

        private async Task DecompressAttachment(Attachment attachment)
        {
            _logger.Debug($"{_internalMessage.Prefix} Attachment {attachment.Id} will be Decompressed");

            attachment.Content.Position = 0;

            VirtualStream outputStream;

            if (attachment.Content.CanSeek)
            {
                outputStream = VirtualStream.CreateVirtualStream(expectedSize: attachment.Content.Length);
            }
            else
            {
                outputStream = new VirtualStream();
            }

            using (var gzipCompression = new GZipStream(
                attachment.Content, CompressionMode.Decompress, leaveOpen: true))
            {
                await gzipCompression.CopyToAsync(outputStream);
                outputStream.Position = 0;
                attachment.Content = outputStream;
            }
        }

        private static void AssignAttachmentProperties(List<PartInfo> messagePayloadInfo, Attachment attachment)
        {
            attachment.Properties["CompressionType"] = GzipContentType;
            string mimeType = GetMimeType(messagePayloadInfo, attachment);

            if (String.IsNullOrWhiteSpace(mimeType))
            {
                throw new InvalidDataException($"MimeType is not specified for attachment {attachment.Id}");
            }

            if (mimeType.IndexOf("/", StringComparison.OrdinalIgnoreCase) < 0)
            {
                throw new InvalidDataException($"Invalid MimeType specified for attachment {attachment.Id}");
            }

            attachment.Properties["MimeType"] = mimeType;
            attachment.ContentType = mimeType;
        }

        private static string GetMimeType(List<PartInfo> messagePayloadInfo, Attachment attachment)
        {
            return messagePayloadInfo.Find(i => i.Href.Equals("cid:" + attachment.Id)).Properties["MimeType"];
        }

        private static AS4Exception ThrowAS4CannotDecompressException(AS4Message as4Message, Exception exception)
        {
            string description = $"Cannot decompress the message: {exception.Message}";
            LogManager.GetCurrentClassLogger().Error(description);

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