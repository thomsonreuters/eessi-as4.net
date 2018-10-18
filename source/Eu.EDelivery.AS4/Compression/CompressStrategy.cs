using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Streaming;
using NLog;

namespace Eu.EDelivery.AS4.Compression
{
    internal class CompressStrategy
    {
        public const string CompressionType = "application/gzip";

        private readonly IEnumerable<Attachment> _attachments;
        private readonly IEnumerable<PartInfo> _partInfos;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressStrategy"/> class.
        /// </summary>
        public CompressStrategy(
            IEnumerable<Attachment> attachments, 
            IEnumerable<PartInfo> partInfos)
        {
            if (attachments == null)
            {
                throw new ArgumentNullException(nameof(attachments));
            }

            if (attachments.Any(a => a is null))
            {
                throw new ArgumentNullException(nameof(attachments), @"AS4Message.Attachments contains a 'null' instance");
            }

            if (partInfos == null)
            {
                throw new ArgumentNullException(nameof(partInfos));
            }

            if (partInfos.Any(p => p is null))
            {
                throw new ArgumentNullException(nameof(partInfos), @"AS4Message.UserMessage.PartInfos contains a 'null' instance");
            }

            _attachments = attachments;
            _partInfos = partInfos;
        }

        public static CompressStrategy ForAS4Message(AS4Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.UserMessages.Any(u => u is null))
            {
                throw new ArgumentNullException(
                    nameof(message.UserMessages),
                    @"AS4Message.UserMessages. contains a 'null' instance");
            }

            return new CompressStrategy(
                message.Attachments, 
                message.UserMessages.SelectMany(u => u.PayloadInfo));
        }

        /// <summary>
        /// Compresses the Attachments that are part of this AS4 Message and
        /// modifies the Payload-info in the UserMessage to indicate that the attachment 
        /// is compressed.
        /// </summary>
        public void Compress()
        {
            if (!_attachments.Any())
            {
                Logger.Debug("No attachments present in AS4Message to compress");
                return;
            }

            foreach (Attachment attachment in _attachments)
            {
                CompressAttachment(attachment);
            }
        }

        private static void CompressAttachment(Attachment attachment)
        {
            VirtualStream outputStream =
                VirtualStream.Create(
                    attachment.EstimatedContentSize > -1 
                        ? attachment.EstimatedContentSize 
                        : VirtualStream.ThresholdMax);

            CompressionLevel compressionLevel = DetermineCompressionLevelFor(attachment);

            using (var gzipCompression = new GZipStream(outputStream, compressionLevel, leaveOpen: true))
            {
                attachment.Content.CopyTo(gzipCompression);
            }

            outputStream.Position = 0;
            attachment.MimeType = attachment.ContentType;
            attachment.CompressionType = CompressionType;
            attachment.UpdateContent(outputStream, CompressionType);
        }

        private static CompressionLevel DetermineCompressionLevelFor(Attachment attachment)
        {
            if (attachment.ContentType.Equals(CompressionType, StringComparison.OrdinalIgnoreCase))
            {
                // In certain cases, we do not want to waste time compressing the attachment, since
                // compressing will only take time without noteably decreasing the attachment size.
                return CompressionLevel.Fastest;
            }

            if (attachment.EstimatedContentSize > -1)
            {
                const long twelveKilobytes = 12_288;
                const long twoHundredMegabytes = 209_715_200;

                if (attachment.EstimatedContentSize <= twelveKilobytes ||
                    attachment.EstimatedContentSize > twoHundredMegabytes )
                {
                    return CompressionLevel.Fastest;
                }
            }

            return CompressionLevel.Optimal;
        }

        /// <summary>
        /// Decompresses the Attachments that are part of this AS4 Message.
        /// </summary>
        public void Decompress()
        {
            if (!_attachments.Any())
            {
                Logger.Debug("No attachments present in AS4Message to decompress");
                return;
            }

            foreach (Attachment attachment in _attachments)
            {
                if (!attachment.IsCompressed)
                {
                    Logger.Debug($"Skip Attachment {attachment.Id} because it's not compressed");
                    continue;
                }

                if (!attachment.HasMimeType)
                {
                    throw new InvalidDataException(
                        $"Cannot decompress attachment \"{attachment.Id}\" because it hasn't got a PartProperty called \"MimeType\"");
                }

                Logger.Trace($"Attachment {attachment.Id} will be decompressed");
                DecompressAttachment(_partInfos, attachment);
                Logger.Debug($"Attachment {attachment.Id} is decompressed to a type of {attachment.ContentType}");
            }
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

            attachment.CompressionType = CompressionType;
            string mimeType = payloadInfo.FirstOrDefault(attachment.Matches)?.Properties["MimeType"];

            if (String.IsNullOrWhiteSpace(mimeType))
            {
                throw new InvalidDataException(
                    $"Cannot decompress attachment {attachment.Id}: MimeType is not specified in referenced <PartInfo/> element");
            }

            if (mimeType.IndexOf("/", StringComparison.OrdinalIgnoreCase) < 0)
            {
                throw new InvalidDataException(
                    $"Cannot decompress attachment {attachment.Id}: Invalid MimeType {mimeType} in referenced <PartInfo/> element");
            }

            attachment.MimeType = mimeType;
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
