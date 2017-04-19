using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Streaming;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how the AS4 UserMessage gets compressed
    /// </summary>
    public class CompressAttachmentsStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private InternalMessage _internalMessage;

        /// <summary>
        /// Compress the <see cref="AS4Message" /> if required
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            if (!internalMessage.AS4Message.SendingPMode.MessagePackaging.UseAS4Compression)
            {
                return await ReturnSameInternalMessage(internalMessage);
            }

            _internalMessage = internalMessage;
            await TryCompressAS4MessageAsync(internalMessage.AS4Message.Attachments);

            return await StepResult.SuccessAsync(internalMessage);
        }

        private static async Task<StepResult> ReturnSameInternalMessage(InternalMessage internalMessage)
        {
            Logger.Debug($"Sending PMode {internalMessage.AS4Message.SendingPMode.Id} Compression is disabled");
            return await StepResult.SuccessAsync(internalMessage);
        }

        private async Task TryCompressAS4MessageAsync(IEnumerable<Attachment> attachments)
        {
            try
            {
                Logger.Info(
                    $"{_internalMessage.Prefix} Compress AS4 Message Attachments with GZip Compression");
                await CompressAttachments(attachments);
            }
            catch (SystemException exception)
            {
                throw ThrowAS4CompressingException(exception);
            }
        }

        private static async Task CompressAttachments(IEnumerable<Attachment> attachments)
        {
            foreach (Attachment attachment in attachments)
            {
                await CompressAttachmentAsync(attachment);
                AssignAttachmentProperties(attachment);
            }
        }

        private static async Task CompressAttachmentAsync(Attachment attachment)
        {
            var outputStream = new VirtualStream();

            using (var gzipCompression = new GZipStream(outputStream, CompressionMode.Compress, leaveOpen: true))
            {
                await attachment.Content.CopyToAsync(gzipCompression);
            }

            outputStream.Position = 0;
            attachment.Content = outputStream;
        }

        private static void AssignAttachmentProperties(Attachment attachment)
        {
            attachment.Properties["CompressionType"] = "application/gzip";
            attachment.Properties["MimeType"] = attachment.ContentType;
            attachment.ContentType = "application/gzip";
        }

        private AS4Exception ThrowAS4CompressingException(Exception innerException)
        {
            string description = $"{_internalMessage.Prefix} Attachments cannot be compressed";
            Logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithInnerException(innerException)
                .WithMessageIds(_internalMessage.AS4Message.MessageIds)
                .WithSendingPMode(_internalMessage.AS4Message.SendingPMode)
                .Build();
        }
    }
}