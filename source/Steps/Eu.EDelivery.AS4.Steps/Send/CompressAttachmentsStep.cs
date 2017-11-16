using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Streaming;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how the attachments of an AS4 message must be compressed.
    /// </summary>
    [Description("This step compresses the attachments of an AS4 Message if compression is enabled in the sending PMode.")]
    [Info("Compress attachments")]
    public class CompressAttachmentsStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private MessagingContext _messagingContext;

        /// <summary>
        /// Compress the <see cref="AS4Message" /> if required
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            if (!messagingContext.SendingPMode.MessagePackaging.UseAS4Compression)
            {
                return ReturnSameMessagingContext(messagingContext);
            }

            _messagingContext = messagingContext;
            TryCompressAS4MessageAsync(messagingContext.AS4Message.Attachments);

            return await StepResult.SuccessAsync(messagingContext);
        }

        private static StepResult ReturnSameMessagingContext(MessagingContext messagingContext)
        {
            Logger.Debug($"Sending PMode {messagingContext.SendingPMode.Id} Compression is disabled");
            return StepResult.Success(messagingContext);
        }

        private void TryCompressAS4MessageAsync(IEnumerable<Attachment> attachments)
        {
            try
            {
                Logger.Info(
                    $"{_messagingContext.EbmsMessageId} Compress AS4 Message Attachments with GZip Compression");
                CompressAttachments(attachments);
            }
            catch (SystemException exception)
            {
                throw ThrowAS4CompressingException(exception);
            }
        }

        private static void CompressAttachments(IEnumerable<Attachment> attachments)
        {
            foreach (Attachment attachment in attachments)
            {
                CompressAttachmentAsync(attachment);
                AssignAttachmentProperties(attachment);
            }
        }

        private static void CompressAttachmentAsync(Attachment attachment)
        {
            VirtualStream outputStream =
                VirtualStream.CreateVirtualStream(
                    attachment.Content.CanSeek ? attachment.Content.Length : VirtualStream.ThresholdMax);

            var compressionLevel = DetermineCompressionLevelFor(attachment);

            var sw = new Stopwatch();
            sw.Start();

            using (var gzipCompression = new GZipStream(outputStream, compressionLevel: compressionLevel, leaveOpen: true))
            {
                attachment.Content.CopyTo(gzipCompression);
            }

            sw.Stop();
            Logger.Trace($"Compress took {sw.ElapsedMilliseconds} milliseconds");

            outputStream.Position = 0;
            attachment.Content = outputStream;
        }

        private static CompressionLevel DetermineCompressionLevelFor(Attachment attachment)
        {
            if (attachment.ContentType.Equals("application/gzip", StringComparison.OrdinalIgnoreCase))
            {
                // In certain cases, we do not want to waste time compressing the attachment, since
                // compressing will only take time without noteably decreasing the attachment size.
                return CompressionLevel.NoCompression;
            }

            if (attachment.Content.CanSeek)
            {
                const long twelveKilobytes = 12_288;
                const long twoHundredMegabytes = 209_715_200;

                if (attachment.Content.Length <= twelveKilobytes)
                {
                    return CompressionLevel.NoCompression;
                }

                if (attachment.Content.Length > twoHundredMegabytes)
                {
                    return CompressionLevel.Fastest;
                }
            }

            return CompressionLevel.Optimal;
        }

        private static void AssignAttachmentProperties(Attachment attachment)
        {
            attachment.Properties["CompressionType"] = "application/gzip";
            attachment.Properties["MimeType"] = attachment.ContentType;
            attachment.ContentType = "application/gzip";
        }

        private static Exception ThrowAS4CompressingException(Exception innerException)
        {
            const string description = "Attachments cannot be compressed";
            Logger.Error(description);

            return new InvalidDataException(description, innerException);
        }
    }
}