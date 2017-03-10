using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Deliver
{
    /// <summary>
    /// <see cref="IStep"/> implementation to .zip the attachments to one file
    /// </summary>
    public class ZipAttachmentsStep : IStep
    {
        private readonly ILogger _logger;
        private readonly IMimeTypeRepository _repository;
        private AS4Message _as4Message;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipAttachmentsStep"/> class
        /// </summary>
        public ZipAttachmentsStep()
        {
            this._repository = new MimeTypeRepository();
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start zipping <see cref="Attachment"/> Models
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._as4Message = internalMessage.AS4Message;

            if (HasAS4MessageMultipleAttachments()) ZipAttachments();

            this._logger.Info($"{internalMessage.Prefix} Zip the Attachments to a single file");
            return await StepResult.SuccessAsync(internalMessage);
        }

        private bool HasAS4MessageMultipleAttachments()
        {
            return this._as4Message.Attachments.Count > 1;
        }

        private void ZipAttachments()
        {
            var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
                CreateAttachmentEntries(archive);

            Attachment zipAttachment = CreateZippedAttachment(memoryStream);
            OverrideAS4MessageAttachments(zipAttachment);
        }

        private void CreateAttachmentEntries(ZipArchive archive)
        {
            foreach (Attachment attachment in this._as4Message.Attachments)
                CreateAttachmentEntry(archive, attachment);
        }

        private void CreateAttachmentEntry(ZipArchive archive, Attachment attachment)
        {
            string entryName = GetAttachmentEntryName(attachment);
            ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
            AddAttachmentStreamToEntry(attachment.Content, entry);
        }

        private string GetAttachmentEntryName(Attachment attachment)
        {
            return attachment.Id + this._repository.GetExtensionFromMimeType(attachment.ContentType);
        }

        private static void AddAttachmentStreamToEntry(Stream attachmentStream, ZipArchiveEntry entry)
        {
            using (Stream entryStream = entry.Open())
                attachmentStream.CopyTo(entryStream);
        }

        private static Attachment CreateZippedAttachment(Stream stream)
        {
            stream.Position = 0;

            return new Attachment
            {
                ContentType = "application/zip",
                Content = stream
            };
        }

        private void OverrideAS4MessageAttachments(Attachment zipAttachment)
        {
            this._as4Message.Attachments.Clear();
            this._as4Message.Attachments.Add(zipAttachment);
        }
    }
}
