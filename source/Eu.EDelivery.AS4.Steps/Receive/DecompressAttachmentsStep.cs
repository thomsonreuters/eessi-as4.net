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
                Logger.Debug("No decompression will happen because the incoming AS4Message must be forwarded unchanged");
                return StepResult.Success(messagingContext);
            }

            if (messagingContext.AS4Message.HasAttachments == false)
            {
                Logger.Debug("No decompression will happen because the AS4Message hasn't got any attachments to decompress");
                return StepResult.Success(messagingContext);
            }

            if (messagingContext.AS4Message.IsEncrypted)
            {
                Logger.Warn("Incoming attachmets are still encrypted will fail to decompress correctly");
            }

            return await TryDecompressAttachmentsAsync(messagingContext).ConfigureAwait(false);
        }

        private static async Task<StepResult> TryDecompressAttachmentsAsync(MessagingContext context)
        {
            try
            {
                DecompressAttachments(context.AS4Message);
                return await StepResult.SuccessAsync(context);
            }
            catch (Exception exception)
            when (
                exception is ArgumentException
                || exception is ObjectDisposedException
                || exception is InvalidDataException)
            {
                if (context.AS4Message.IsEncrypted)
                {
                    Logger.Error(
                        "Decompression failed because the incoming attachments are still encrypted, "
                        + "make sure that you specify <Encryption/> information in the <Security/> element of the SendingPMode "
                        + "so the attachments gets first decrypted before decompressed");
                }

                context.ErrorResult = new ErrorResult(exception.Message, ErrorAlias.DecompressionFailure);
                return StepResult.Failed(context);
            }
        }

        private static void DecompressAttachments(AS4Message as4Message)
        {
            IEnumerable<PartInfo> partInfos = as4Message.UserMessages.SelectMany(u => u.PayloadInfo);
            foreach (Attachment attachment in as4Message.Attachments)
            {
                if (IsAttachmentNotCompressed(attachment))
                {
                    Logger.Debug($"Attachment {attachment.Id} is not compressed, so can't be decompressed");
                    continue;
                }

                if (!attachment.Properties.ContainsKey("MimeType"))
                {
                    throw new InvalidDataException(
                        $"Cannot decompress attachment \"{attachment.Id}\" because it hasn't got a PartProperty called \"MimeType\"");
                }

                Logger.Trace($"Attachment {attachment.Id} will be decompressed");
                DecompressAttachment(partInfos, attachment);
                Logger.Debug($"Attachment {attachment.Id} is decompressed to a type of {attachment.ContentType}");
            }
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
                    "MimeType is not specified in referenced <PartInfo/> element, please provide one");
            }

            if (mimeType.IndexOf("/", StringComparison.OrdinalIgnoreCase) < 0)
            {
                throw new InvalidDataException(
                    $"Cannot decompress attachment {attachment.Id}: " + 
                    $"Invalid MimeType {mimeType} in referenced <PartInfo/> element");
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