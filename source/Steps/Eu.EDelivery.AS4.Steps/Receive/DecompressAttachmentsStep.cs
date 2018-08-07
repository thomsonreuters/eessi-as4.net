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
            IEnumerable<PartInfo> partInfos = context.AS4Message.UserMessages.SelectMany(u => u.PayloadInfo);
            foreach (Attachment attachment in context.AS4Message.Attachments)
            {
                if (IsAttachmentNotCompressed(attachment))
                {
                    Logger.Debug($" Attachment {attachment.Id} is not compressed, so can't be decompressed");
                    continue;
                }

                if (!attachment.Properties.ContainsKey("MimeType"))
                {
                    return DecompressFailureResult($"Attachment {attachment.Id} hasn't got a MimeType PartProperty", context);
                }

                Logger.Trace($"Attachment {attachment.Id} will be decompressed");
                DecompressAttachment(partInfos, attachment);
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

        private static void DecompressAttachment(IEnumerable<PartInfo> payloadInfo, Attachment attachment)
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

            Stream decompressed = DecompressStream(attachment.Content);

            attachment.Properties["CompressionType"] = GzipContentType;
            string mimeType = payloadInfo.FirstOrDefault(attachment.Matches)?.Properties["MimeType"];

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
            attachment.UpdateContent(decompressed, mimeType);
        }

        private static Stream DecompressStream(Stream input)
        {
            long unzipLength = StreamUtilities.DetermineOriginalSizeOfCompressedStream(input);

            VirtualStream outputStream =
                VirtualStream.Create(
                    unzipLength > -1 ? unzipLength : VirtualStream.ThresholdMax);

            if (unzipLength > 0)
            {
                outputStream.SetLength(unzipLength);
            }

            using (var gzipCompression = new GZipStream(input, CompressionMode.Decompress, true))
            {
                gzipCompression.CopyTo(outputStream);
                outputStream.Position = 0;
                return outputStream;
            }
        }
    }
}