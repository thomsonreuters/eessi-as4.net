using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Utilities;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Uploader
{
    /// <summary>
    /// <see cref="IAttachmentUploader" /> implementation to upload attachments to the file system
    /// </summary>
    [Info("FILE")]
    public class FileAttachmentUploader : IAttachmentUploader
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IMimeTypeRepository _repository;

        private Method _method;
        [Info("location")]
        private string Location => _method["location"]?.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAttachmentUploader" /> class.
        /// Create a Payload Uploader for the file system
        /// </summary>
        /// <param name="repository">
        /// </param>
        public FileAttachmentUploader(IMimeTypeRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Configure the <see cref="IAttachmentUploader" />
        /// with a given <paramref name="payloadReferenceMethod" />
        /// </summary>
        /// <param name="payloadReferenceMethod"></param>
        public void Configure(Method payloadReferenceMethod)
        {
            _method = payloadReferenceMethod;
        }

        /// <summary>
        /// Start uploading Attachment
        /// </summary>
        /// <param name="attachment"></param>
        /// <returns></returns>
        public async Task<UploadResult> UploadAsync(Attachment attachment)
        {
            string downloadUrl = AssembleFileDownloadUrlFor(attachment);
            string attachmentFilePath = Path.GetFullPath(downloadUrl);

            await TryUploadAttachment(attachment, attachmentFilePath).ConfigureAwait(false);
            return new UploadResult { DownloadUrl = attachmentFilePath };
        }

        private string AssembleFileDownloadUrlFor(Attachment attachment)
        {
            string extension = _repository.GetExtensionFromMimeType(attachment.ContentType);
            string fileName = FilenameSanitizer.EnsureValidFilename(attachment.Id);

            return Path.Combine(Location, $"{fileName}{extension}");
        }

        private static async Task TryUploadAttachment(Attachment attachment, string attachmentFilePath)
        {
            try
            {
                await UploadAttachment(attachment, attachmentFilePath).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occured while uploading the attachment: {ex.Message}");
                string description = $"Unable to upload attachment {attachment.Id} to {attachment.Location}";

                throw new IOException(description, ex);
            }
        }

        private static async Task UploadAttachment(Attachment attachment, string attachmentFilePath)
        {
            // Create the directory, if it does not exist.
            Directory.CreateDirectory(Path.GetDirectoryName(attachmentFilePath));

            using (FileStream fileStream = File.Create(attachmentFilePath))
            {
                await attachment.Content.CopyToAsync(fileStream).ConfigureAwait(false);
            }

            Logger.Info($"Attachment {attachment.Id} is uploaded successfully to {attachment.Location}");
        }
    }
}