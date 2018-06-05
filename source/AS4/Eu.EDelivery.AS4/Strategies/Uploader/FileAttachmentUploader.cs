using System;
using System.ComponentModel;
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

        [Info("Location")]
        [Description("Folder where the payloads must be delivered")]
        private string Location => _method["location"]?.Value;

        [Info("Payload Naming Pattern")]
        [Description(PayloadFileNameFactory.PatternDocumentation)]
        private string NamePattern => _method["filenameformat"]?.Value;

        [Info("Allow overwrite")]
        [Description(
            "When Allow overwrite is set to true, the file will be overwritten if it already exists.\n\r" +
            "When set to false, an attempt will be made to create a new unique filename. The default is false.")]
        private string AllowOverwrite => _method["allowoverwrite"]?.Value;

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
        public Task<UploadResult> UploadAsync(Attachment attachment, UserMessage referringUserMessage)
        {
            string downloadUrl = AssembleFileDownloadUrlFor(attachment, referringUserMessage);
            if (downloadUrl == null)
            {
                return Task.FromResult(UploadResult.FatalFail);
            }

            string attachmentFilePath = Path.GetFullPath(downloadUrl);

            bool allowOverwrite = DetermineAllowOverwrite();
            return Task.FromResult(TryUploadAttachment(attachment, attachmentFilePath, allowOverwrite));
        }

        private string AssembleFileDownloadUrlFor(Attachment attachment, UserMessage referringUserMessage)
        {
            try
            {
                string extension = _repository.GetExtensionFromMimeType(attachment.ContentType);

                string fileName = PayloadFileNameFactory.CreateFileName(NamePattern, attachment, referringUserMessage);

                fileName = FilenameUtils.EnsureValidFilename($"{fileName}{extension}");

                return Path.Combine(Location, fileName);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"(Deliver)[{referringUserMessage.MessageId}] An fatal error occured while determining the file path: {ex}");

                return null;
            }
        }

        private bool DetermineAllowOverwrite()
        {
            if (String.IsNullOrEmpty(AllowOverwrite))
            {
                return false;
            }

            if (Boolean.TryParse(AllowOverwrite, out bool allowOverwrite))
            {
                return allowOverwrite;
            }

            return false;
        }

        private static UploadResult TryUploadAttachment(Attachment attachment, string attachmentFilePath, bool allowOverwrite)
        {
            try
            {
                try
                {
                    Task<UploadResult> t = UploadAttachment(attachment, attachmentFilePath, allowOverwrite);
                    t.Wait();

                    return t.Result;
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
                    return TryUploadAttachment(attachment, attachmentFilePath, allowOverwrite);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error(
                    $"(Deliver) A fatal error occured while uploading the attachment {attachment.Id}: {ex.Message}");

                return UploadResult.FatalFail;
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"(Deliver) An error occured while uploading the attachment {attachment.Id}: {ex.Message}, will be retried");

                return UploadResult.RetryableFail;
            }
        }

        private static async Task<UploadResult> UploadAttachment(Attachment attachment, string attachmentFilePath, bool overwriteExisting)
        {
            // Create the directory, if it does not exist.
            Directory.CreateDirectory(Path.GetDirectoryName(attachmentFilePath));

            FileMode mode = FileMode.Create;

            if (overwriteExisting == false)
            {
                attachmentFilePath = FilenameUtils.EnsureFilenameIsUnique(attachmentFilePath);
                mode = FileMode.CreateNew;
            }

            Logger.Trace($"(Deliver) Trying to upload attachment {attachment.Id} to {attachmentFilePath}");

            using (FileStream fileStream = new FileStream(attachmentFilePath, mode, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await attachment.Content.CopyToFastAsync(fileStream).ConfigureAwait(false);
            }

            Logger.Info($"(Deliver) Attachment {attachment.Id} is uploaded successfully to {attachmentFilePath}");

            return UploadResult.SuccessWithUrl(attachmentFilePath);
        }
    }
}