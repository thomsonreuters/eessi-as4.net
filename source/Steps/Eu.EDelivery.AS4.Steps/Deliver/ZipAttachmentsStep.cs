using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Streaming;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Deliver
{
    /// <summary>
    /// <see cref="IStep"/> implementation to .zip the attachments to one file
    /// </summary>
    public class ZipAttachmentsStep : IStep
    {        
        private readonly IMimeTypeRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipAttachmentsStep"/> class
        /// </summary>
        public ZipAttachmentsStep()
        {
            _repository = new MimeTypeRepository();            
        }

        /// <summary>
        /// Start zipping <see cref="Attachment"/> Models
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {           
            if (HasAS4MessageMultipleAttachments(internalMessage.AS4Message))
            {
                Stream zippedStream = await ZipAttachmentsInAS4Message(internalMessage.AS4Message).ConfigureAwait(false);

                Attachment zipAttachment = CreateZippedAttachment(zippedStream);

                OverwriteAttachmentEntries(internalMessage.AS4Message, zipAttachment);
            }

            LogManager.GetCurrentClassLogger().Info($"{internalMessage.Prefix} Zip the Attachments to a single file");

            return await StepResult.SuccessAsync(internalMessage).ConfigureAwait(false);
        }

        private static bool HasAS4MessageMultipleAttachments(AS4Message as4Message)
        {
            return as4Message.Attachments.Count > 1;
        }

        private async Task<Stream> ZipAttachmentsInAS4Message(AS4Message message)
        {
            var stream = new VirtualStream();

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {                
                foreach (Attachment attachment in message.Attachments)
                {
                    ZipArchiveEntry archiveEntry = CreateAttachmentEntry(archive, attachment);
                    await AddAttachmentStreamToEntry(attachment.Content, archiveEntry).ConfigureAwait(false);
                }
            }

            stream.Position = 0;

            return stream;
        }
        
        private ZipArchiveEntry CreateAttachmentEntry(ZipArchive archive, Attachment attachment)
        {
            string entryName = GetAttachmentEntryName(attachment);
            ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
            return entry;            
        }

        private string GetAttachmentEntryName(Attachment attachment)
        {
            return attachment.Id + _repository.GetExtensionFromMimeType(attachment.ContentType);
        }

        private static async Task AddAttachmentStreamToEntry(Stream attachmentStream, ZipArchiveEntry entry)
        {
            using (Stream entryStream = entry.Open())
            {
                await attachmentStream.CopyToAsync(entryStream).ConfigureAwait(false);
            }
        }

        private static Attachment CreateZippedAttachment(Stream stream)
        {           
            return new Attachment
            {
                ContentType = "application/zip",
                Content = stream
            };
        }

        private static void OverwriteAttachmentEntries(AS4Message message, Attachment zipAttachment)
        {
            message.Attachments.Clear();
            message.Attachments.Add(zipAttachment);
        }        
    }
}
