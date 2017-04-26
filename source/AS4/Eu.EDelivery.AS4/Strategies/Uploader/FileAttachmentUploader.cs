using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
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
    public class FileAttachmentUploader : IAttachmentUploader
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IMimeTypeRepository _repository;

        private Method _method;

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

            await TryUploadAttachment(attachment, attachmentFilePath);
            return new UploadResult {DownloadUrl = attachmentFilePath};
        }

        private string AssembleFileDownloadUrlFor(Attachment attachment)
        {
            Parameter locationParameter = _method["location"];

            string extension = _repository.GetExtensionFromMimeType(attachment.ContentType);
            string fileName = FilenameSanitizer.EnsureValidFilename(attachment.Id);

            return $"{locationParameter.Value}{fileName}{extension}";
        }

        private static async Task TryUploadAttachment(Attachment attachment, string attachmentFilePath)
        {
            try
            {
                await UploadAttachment(attachment, attachmentFilePath);
            }
            catch (SystemException ex)
            {
                Logger.Error($"An error occured while uploading the attachment: {ex.Message}");
                throw ThrowAS4UploadException($"Unable to upload attachment {attachment.Id} to {attachment.Location}");
            }
        }

        private static AS4Exception ThrowAS4UploadException(string description)
        {
            Logger.Info(description);
            return new AS4Exception(description);
        }

        private static async Task UploadAttachment(Attachment attachment, string attachmentFilePath)
        {
            // Create the directory, if it does not exist.
            Directory.CreateDirectory(Path.GetDirectoryName(attachmentFilePath));

            using (FileStream fileStream = File.Create(attachmentFilePath))
            {
                await attachment.Content.CopyToAsync(fileStream);
            }

            Logger.Info($"Attachment {attachment.Id} is uploaded successfully to {attachment.Location}");
        }
    }
}