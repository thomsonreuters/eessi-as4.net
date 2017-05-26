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
                return await ReturnSameInternalMessage(messagingContext);
            }

            _messagingContext = messagingContext;
            await TryCompressAS4MessageAsync(messagingContext.AS4Message.Attachments).ConfigureAwait(false);

            return await StepResult.SuccessAsync(messagingContext);
        }

        private static async Task<StepResult> ReturnSameInternalMessage(MessagingContext messagingContext)
        {
            Logger.Debug($"Sending PMode {messagingContext.SendingPMode.Id} Compression is disabled");
            return await StepResult.SuccessAsync(messagingContext);
        }

        private async Task TryCompressAS4MessageAsync(IEnumerable<Attachment> attachments)
        {
            try
            {
                Logger.Info(
                    $"{_messagingContext.Prefix} Compress AS4 Message Attachments with GZip Compression");
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
                await CompressAttachmentAsync(attachment).ConfigureAwait(false);
                AssignAttachmentProperties(attachment);
            }
        }

        private static async Task CompressAttachmentAsync(Attachment attachment)
        {
            var outputStream = VirtualStream.CreateVirtualStream(expectedSize: (attachment.Content.CanSeek) ? attachment.Content.Length : VirtualStream.ThresholdMax);

            using (var gzipCompression = new GZipStream(outputStream, CompressionMode.Compress, leaveOpen: true))
            {
                await attachment.Content.CopyToAsync(gzipCompression).ConfigureAwait(false);
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
            string description = $"{_messagingContext.Prefix} Attachments cannot be compressed";
            Logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithInnerException(innerException)
                .WithMessageIds(_messagingContext.AS4Message.MessageIds)
                .WithSendingPMode(_messagingContext.SendingPMode)
                .Build();
        }
    }
}