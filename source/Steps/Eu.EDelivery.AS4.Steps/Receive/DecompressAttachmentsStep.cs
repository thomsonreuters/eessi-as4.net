using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
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
    [Description("If necessary, decompresses the attachments that are present in the received message.")]
    [Info("Decompress attachments")]
    public class DecompressAttachmentsStep : IStep
    {
        private const string GzipContentType = "application/gzip";

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Decompress any Attachments
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            if (messagingContext.ReceivedMessageMustBeForwarded)
            {
                // When the message must be forwarded, no decompression must take place.
                return StepResult.Success(messagingContext);
            }

            if (messagingContext.AS4Message.HasAttachments == false)
            {
                Logger.Debug($"[{messagingContext.AS4Message.GetPrimaryMessageId()}] AS4Message hasn't got any Attachments to decompress");
                return StepResult.Success(messagingContext);
            }

            return await TryDecompressAttachments(messagingContext).ConfigureAwait(false);
        }

        private static async Task<StepResult> TryDecompressAttachments(MessagingContext context)
        {
            try
            {
                return await DecompressAttachments(context).ConfigureAwait(false);
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

        private static async Task<StepResult> DecompressAttachments(MessagingContext context)
        {
            AS4Message as4Message = context.AS4Message;

            Logger.Info($"[{as4Message.GetPrimaryMessageId()}] Decompressing attachments");

            foreach (Attachment attachment in as4Message.Attachments)
            {
                if (IsAttachmentNotCompressed(attachment))
                {
                    Logger.Debug($"[{as4Message.GetPrimaryMessageId()}] Attachment {attachment.Id} is not Compressed");
                    continue;
                }

                if (HasntAttachmentMimeTypePartProperty(attachment))
                {
                    string description = $"Attachment {attachment.Id} hasn't got a MimeType PartProperty";
                    return DecompressFailureResult(description, context);
                }

                Logger.Debug($"[{as4Message.GetPrimaryMessageId()}] Attachment {attachment.Id} will be Decompressed");
                DecompressAttachment(attachment);
                AssignAttachmentProperties(as4Message.PrimaryUserMessage.PayloadInfo.ToList(), attachment);
            }

            Logger.Info($"[{as4Message.GetPrimaryMessageId()}] Attachments decompressed");
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
                VirtualStream.CreateVirtualStream(
                    unzipLength > -1 ? unzipLength : VirtualStream.ThresholdMax);

            if (unzipLength > 0)
            {
                outputStream.SetLength(unzipLength);
            }

            var sw = new Stopwatch();
            sw.Start();

            using (var gzipCompression = new GZipStream(attachment.Content, CompressionMode.Decompress, true))
            {
                gzipCompression.CopyTo(outputStream);
                outputStream.Position = 0;
                attachment.Content = outputStream;
            }

            sw.Stop();
            Logger.Trace($"Decompress copytofastasync took {sw.ElapsedMilliseconds} millisecs");
        }

        private static void AssignAttachmentProperties(List<PartInfo> messagePayloadInfo, Attachment attachment)
        {
            attachment.Properties["CompressionType"] = GzipContentType;
            string mimeType = messagePayloadInfo.Find(attachment.Matches).Properties["MimeType"];

            if (string.IsNullOrWhiteSpace(mimeType))
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
    }
}