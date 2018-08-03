using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Streaming;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Decompress the incoming Payloads
    /// </summary>
    [Info("Decompress attachments")]
    [Description("If necessary, decompresses the attachments that are present in the received message.")]
    public class DecompressAttachmentsStep : IStep
    {
        private const string GzipContentType = "application/gzip";

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Decompress any Attachments
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (messagingContext.AS4Message == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(DecompressAttachmentsStep)} requires a AS4Message but no AS4Message is present in the MessagingContext");
            }

            if (messagingContext.ReceivedMessageMustBeForwarded)
            {
                // When the message must be forwarded, no decompression must take place.
                Logger.Debug(
                    "Because the incoming AS4Message must be forwarded, " +
                    "we can't alter the message. So, no decompression will take place");

                return StepResult.Success(messagingContext);
            }

            if (messagingContext.AS4Message.HasAttachments == false)
            {
                Logger.Debug("No decompression will happend because the AS4Message hasn't got any attachments to decompress");
                return StepResult.Success(messagingContext);
            }

            return await TryDecompressAttachmentsAsync(messagingContext).ConfigureAwait(false);
        }

        private static async Task<StepResult> TryDecompressAttachmentsAsync(MessagingContext context)
        {
            try
            {
                return await DecompressAttachmentsAsync(context).ConfigureAwait(false);
            }
            catch (Exception exception)
            when (
                exception is ArgumentException
                || exception is ObjectDisposedException
                || exception is InvalidDataException)
            {
                return DecompressFailureResult(exception.Message, context);
            }
        }

        private static async Task<StepResult> DecompressAttachmentsAsync(MessagingContext context)
        {
            AS4Message as4Message = context.AS4Message;

            foreach (Attachment attachment in as4Message.Attachments)
            {
                if (IsAttachmentNotCompressed(attachment))
                {
                    Logger.Debug($" Attachment {attachment.Id} is not compressed, so can't be decompressed");
                    continue;
                }

                if (HasntAttachmentMimeTypePartProperty(attachment))
                {
                    string description = $"Attachment {attachment.Id} hasn't got a MimeType PartProperty";
                    return DecompressFailureResult(description, context);
                }

                Logger.Trace($"Attachment {attachment.Id} will be decompressed");
                DecompressAttachment(attachment);
                AssignAttachmentProperties(as4Message.FirstUserMessage.PayloadInfo, attachment);
                Logger.Debug($"Attachment {attachment.Id} is decompressed to a type of {attachment.ContentType}");
            }

            return await StepResult.SuccessAsync(context);
        }

        private static StepResult DecompressFailureResult(string description, MessagingContext context)
        {
            context.ErrorResult = new ErrorResult(description, ErrorAlias.DecompressionFailure);
            return StepResult.Failed(context);
        }

        private static bool IsAttachmentNotCompressed(Attachment attachment)
        {
            return !attachment.ContentType.Equals(GzipContentType, StringComparison.OrdinalIgnoreCase) &&
                   !attachment.Properties.ContainsKey("CompressionType");
        }

        private static bool HasntAttachmentMimeTypePartProperty(Attachment attachment)
        {
            return !attachment.Properties.ContainsKey("MimeType");
        }

        private static void DecompressAttachment(Attachment attachment)
        {
            attachment.ResetContentPosition();

            long unzipLength = StreamUtilities.DetermineOriginalSizeOfCompressedStream(attachment.Content);

            VirtualStream outputStream =
                VirtualStream.Create(
                    unzipLength > -1 ? unzipLength : VirtualStream.ThresholdMax);

            if (unzipLength > 0)
            {
                outputStream.SetLength(unzipLength);
            }

            using (var gzipCompression = new GZipStream(attachment.Content, CompressionMode.Decompress, true))
            {
                gzipCompression.CopyTo(outputStream);
                outputStream.Position = 0;
                attachment.Content = outputStream;
            }
        }

        private static void AssignAttachmentProperties(IEnumerable<PartInfo> messagePayloadInfo, Attachment attachment)
        {
            attachment.Properties["CompressionType"] = GzipContentType;
            string mimeType = messagePayloadInfo.FirstOrDefault(attachment.Matches)?.Properties["MimeType"];

            if (string.IsNullOrWhiteSpace(mimeType))
            {
                throw new InvalidDataException(
                    $"Cannot decompress attachment {attachment.Id}: " + 
                    "MimeType is not specified for attachment, please provide one");
            }

            if (mimeType.IndexOf("/", StringComparison.OrdinalIgnoreCase) < 0)
            {
                throw new InvalidDataException(
                    $"Cannot decompress attachment {attachment.Id}: " + 
                    $"Invalid MimeType {mimeType} specified for attachment");
            }

            attachment.Properties["MimeType"] = mimeType;
            attachment.ContentType = mimeType;
        }
    }
}