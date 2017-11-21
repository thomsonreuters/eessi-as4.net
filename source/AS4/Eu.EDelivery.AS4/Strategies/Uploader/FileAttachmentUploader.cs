using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Streaming;
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

        [Info("Payload Naming Pattern")]
        [Description(PayloadFileNameFactory.PatternDocumentation)]
        private string NamePattern => _method["filenameformat"]?.Value;

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

        /// <inheritdoc/>
        public async Task<UploadResult> UploadAsync(Attachment attachment, UserMessage referringUserMessage)
        {
            string downloadUrl = AssembleFileDownloadUrlFor(attachment, referringUserMessage);
            string attachmentFilePath = Path.GetFullPath(downloadUrl);

            string uploadLocation = await TryUploadAttachment(attachment, attachmentFilePath).ConfigureAwait(false);
            return new UploadResult { DownloadUrl = uploadLocation };
        }

        private string AssembleFileDownloadUrlFor(Attachment attachment, UserMessage referringUserMessage)
        {
            string extension = _repository.GetExtensionFromMimeType(attachment.ContentType);

            string fileName = PayloadFileNameFactory.CreateFileName(NamePattern, attachment, referringUserMessage);

            fileName = FilenameUtils.EnsureValidFilename($"{fileName}{extension}");

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
                // Filter IOExceptions on a specific HResult.
                // -2147024816 is the HResult if the IOException is thrown because the file already exists.
                catch (IOException ex) when (ex.HResult == -2147024816)
                {
                    Logger.Info(ex.Message);

                    // If we happen to be in a concurrent scenario where there already
                    // exists a file with the same name, try to upload the file as well.
                    // The TryUploadAttachment method will generate a new name, but it is 
                    // still possible that, under heavy load, another file has been created
                    // with the same name as the unique name that we've generated.
                    // Therefore, retry again.
                    return await TryUploadAttachment(attachment, attachmentFilePath);
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

            var sw = new Stopwatch();
            sw.Start();

            using (FileStream fileStream = FileUtils.CreateNewAsync(attachmentFilePath, FileOptions.SequentialScan))
            {
                await attachment.Content.CopyToFastAsync(fileStream).ConfigureAwait(false);
            }

            sw.Stop();
            Logger.Trace($"Uploading attachment to filesystem took {sw.ElapsedMilliseconds} milliseconds.");

            Logger.Info($"Attachment {attachment.Id} is uploaded successfully to {attachmentFilePath}");

            return attachmentFilePath;
        }
    }
}