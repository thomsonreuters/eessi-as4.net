using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

        private MessagingContext _messagingContext;

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
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="AS4Exception">Throws exception when AS4 Message cannot be decompressed</exception>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            _messagingContext = messagingContext;

            if (!messagingContext.AS4Message.HasAttachments)
            {
                return await ReturnSameInternalMessage(messagingContext).ConfigureAwait(false);
            }

            await TryDecompressAttachments(messagingContext).ConfigureAwait(false);
            return await StepResult.SuccessAsync(messagingContext);
        }

        private async Task<StepResult> ReturnSameInternalMessage(MessagingContext messagingContext)
        {
            _logger.Debug($"{_messagingContext.Prefix} AS4Message hasn't got any Attachments");
            return await StepResult.SuccessAsync(messagingContext);
        }

        private async Task TryDecompressAttachments(MessagingContext message)
        {
            try
            {
                AS4Message as4Message = message.AS4Message;
                await DecompressAttachments(as4Message.PrimaryUserMessage.PayloadInfo.ToList(), as4Message.Attachments).ConfigureAwait(false);
                _logger.Info(
                    $"{_messagingContext.Prefix} Try Decompress AS4 Message Attachments with GZip Compression");
            }
            catch (Exception exception)
            {
                throw ThrowAS4CannotDecompressException(exception, message);
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

                await DecompressAttachment(attachment).ConfigureAwait(false);
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
                _logger.Debug($"{_messagingContext.Prefix} Attachment {attachment.Id} is not Compressed");
            }

            return isNotCompressed;
        }

        private static bool HasntAttachmentMimeTypePartProperty(Attachment attachment)
        {
            return !attachment.Properties.ContainsKey("MimeType");
        }

        private async Task DecompressAttachment(Attachment attachment)
        {
            _logger.Debug($"{_messagingContext.Prefix} Attachment {attachment.Id} will be Decompressed");

            attachment.ResetContentPosition();

            VirtualStream outputStream = VirtualStream.CreateVirtualStream(expectedSize: (attachment.Content.CanSeek) ? attachment.Content.Length : VirtualStream.ThresholdMax);

            using (var gzipCompression = new GZipStream(attachment.Content, CompressionMode.Decompress, leaveOpen: true))
            {
                await gzipCompression.CopyToAsync(outputStream).ConfigureAwait(false);
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
            return messagePayloadInfo.Find(i => attachment.Matches(i)).Properties["MimeType"];
        }

        private static AS4Exception ThrowAS4CannotDecompressException(Exception exception, MessagingContext message)
        {
            AS4Message as4Message = message.AS4Message;
            string description = $"Cannot decompress the message: {exception.Message}";
            LogManager.GetCurrentClassLogger().Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithMessageIds(as4Message.MessageIds)
                .WithErrorCode(ErrorCode.Ebms0303)
                .WithInnerException(exception)
                .WithReceivingPMode(message.ReceivingPMode)
                .Build();
        }
    }
}