using System;
using System.IO;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Utilities;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Uploader
{
    /// <summary>
    /// <see cref="IAttachmentUploader"/> implementation to upload attachments to the file system
    /// </summary>
    public class FileAttachmentUploader : IAttachmentUploader
    {
        private readonly ILogger _logger;
        private readonly IMimeTypeRepository _repository;
        private Method _method;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAttachmentUploader"/> class. 
        /// Create a Payload Uploader for the file system
        /// </summary>
        /// <param name="repository">
        /// </param>
        public FileAttachmentUploader(IMimeTypeRepository repository)
        {
            this._repository = repository;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Configure the <see cref="IAttachmentUploader"/>
        /// with a given <paramref name="payloadReferenceMethod"/>
        /// </summary>
        /// <param name="payloadReferenceMethod"></param>
        public void Configure(Method payloadReferenceMethod)
        {
            this._method = payloadReferenceMethod;
        }

        /// <summary>
        /// Start uploading Attachment
        /// </summary>
        /// <param name="attachment"></param>
        public void Upload(Attachment attachment)
        {
            AssignAttachmentLocation(attachment);
            TryUploadAttachment(attachment);
        }

        private void AssignAttachmentLocation(Attachment attachment)
        {
            Parameter locationParameter = this._method["location"];

            string extension = this._repository.GetExtensionFromMimeType(attachment.ContentType);
            string fileName = FilenameSanitizer.EnsureValidFilename(attachment.Id);

            attachment.Location = $"{locationParameter.Value}{fileName}{extension}";
        }

        private void TryUploadAttachment(Attachment attachment)
        {
            try
            {
                UploadAttachment(attachment);
            }
            catch (SystemException ex)
            {
                _logger.Error($"An error occured while uploading the attachment: {ex.Message}");
                throw ThrowAS4UploadException($"Unable to upload attachment {attachment.Id} to {attachment.Location}");
            }
        }

        private AS4Exception ThrowAS4UploadException(string description)
        {
            this._logger.Info(description);
            return new AS4Exception(description);
        }

        private void UploadAttachment(Attachment attachment)
        {
            // Create the directory, if it does not exist.
            Directory.CreateDirectory(Path.GetDirectoryName(attachment.Location));

            using (FileStream fileStream = File.Create(attachment.Location))
            {
                attachment.Content.CopyTo(fileStream);
            }

            this._logger.Info($"Attachment {attachment.Id} is uploaded successfully to {attachment.Location}");
        }
    }
}
