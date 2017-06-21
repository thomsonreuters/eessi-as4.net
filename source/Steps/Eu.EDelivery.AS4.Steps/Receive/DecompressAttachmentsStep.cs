using System;
using System.Collections.Generic;
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
            if (messagingContext.AS4Message.HasAttachments)
            {
                return await TryDecompressAttachments(messagingContext).ConfigureAwait(false);
            }

            Logger.Debug($"[{messagingContext.AS4Message.GetPrimaryMessageId()}] AS4Message hasn't got any Attachments to decompress");
            return await StepResult.SuccessAsync(messagingContext);
        }

        private static async Task<StepResult> TryDecompressAttachments(MessagingContext context)
        {
            try
            {
                return await DecompressAttachments(context).ConfigureAwait(false);
            }
            catch (ArgumentException exception)
            {
                return DecompressFailureResult(exception.Message, context);
            }
            catch (ObjectDisposedException exception)
            {
                return DecompressFailureResult(exception.Message, context);
            }
            catch (InvalidDataException exception)
            {
                return DecompressFailureResult(exception.Message, context);
            }
        }

        private static async Task<StepResult> DecompressAttachments(MessagingContext context)
        {
            AS4Message as4Message = context.AS4Message;

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
                await DecompressAttachment(attachment).ConfigureAwait(false);
                AssignAttachmentProperties(as4Message.PrimaryUserMessage.PayloadInfo.ToList(), attachment);
            }

            Logger.Info($"[{as4Message.GetPrimaryMessageId()}] Decompress AS4 Message Attachments with GZip Compression");
            return await StepResult.SuccessAsync(context);
        }

        private static StepResult DecompressFailureResult(string description, MessagingContext context)
        {
            context.ErrorResult = new ErrorResult(description, ErrorCode.Ebms0303, ErrorAlias.DecompressionFailure);
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

        private static async Task DecompressAttachment(Attachment attachment)
        {
            attachment.ResetContentPosition();

            VirtualStream outputStream =
                VirtualStream.CreateVirtualStream(
                    attachment.Content.CanSeek ? attachment.Content.Length : VirtualStream.ThresholdMax);

            using (var gzipCompression = new GZipStream(attachment.Content, CompressionMode.Decompress, true))
            {
                await gzipCompression.CopyToAsync(outputStream).ConfigureAwait(false);
                outputStream.Position = 0;
                attachment.Content = outputStream;
            }
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