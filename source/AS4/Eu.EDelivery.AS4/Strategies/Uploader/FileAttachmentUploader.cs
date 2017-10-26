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
    [Info(FileAttachmentUploader.Key)]
    public class FileAttachmentUploader : IAttachmentUploader
    {
        public const string Key = "FILE";

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

            string uploadLocation = await TryUploadAttachment(attachment, attachmentFilePath).ConfigureAwait(false);
            return new UploadResult { DownloadUrl = uploadLocation };
        }

        private string AssembleFileDownloadUrlFor(Attachment attachment)
        {
            string extension = _repository.GetExtensionFromMimeType(attachment.ContentType);
            string fileName = FilenameUtils.EnsureValidFilename($"{attachment.Id}{extension}");

            return Path.Combine(Location, fileName);
        }

        private static async Task<string> TryUploadAttachment(Attachment attachment, string attachmentFilePath)
        {
            try
            {
                try
                {
                    return await UploadAttachment(attachment, attachmentFilePath).ConfigureAwait(false);
                }
                catch (IOException)
                {
                    if (File.Exists(attachmentFilePath))
                    {
                        // If we happen to be in a concurrent scenario where there already
                        // exists a file with the same name, try to upload the file as well.
                        // The TryUploadAttachment method will generate a new name, but it is 
                        // still possible that, under heavy load, another file has been created
                        // with the same name as the unique name that we've generated.
                        // Therefore, retry again.
                        return await TryUploadAttachment(attachment, attachmentFilePath);
                    }

                    throw;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occured while uploading the attachment: {ex.Message}");
                string description = $"Unable to upload attachment {attachment.Id} to {attachment.Location}";

                throw new IOException(description, ex);
            }
        }

        private static async Task<string> UploadAttachment(Attachment attachment, string attachmentFilePath)
        {
            // Create the directory, if it does not exist.
            Directory.CreateDirectory(Path.GetDirectoryName(attachmentFilePath));

            attachmentFilePath = FilenameUtils.EnsureFilenameIsUnique(attachmentFilePath);

            using (FileStream fileStream = File.Create(attachmentFilePath))
            {
                await attachment.Content.CopyToAsync(fileStream).ConfigureAwait(false);
            }

            Logger.Info($"Attachment {attachment.Id} is uploaded successfully to {attachmentFilePath}");

            return attachmentFilePath;
        }
    }
}