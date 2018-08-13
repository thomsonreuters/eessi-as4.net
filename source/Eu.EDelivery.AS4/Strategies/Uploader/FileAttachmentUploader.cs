using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        private string Location => _method?["location"]?.Value;

        [Info("Payload Naming Pattern")]
        [Description(PayloadFileNameFactory.PatternDocumentation)]
        private string NamePattern => _method?["filenameformat"]?.Value;

        [Info("Allow overwrite")]
        [Description(
            "When Allow overwrite is set to true, the file will be overwritten if it already exists.\n\r" +
            "When set to false, an attempt will be made to create a new unique filename. The default is false.")]
        private string AllowOverwrite => _method?["allowoverwrite"]?.Value;

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
                Logger.Debug("No");
                return Task.FromResult(UploadResult.FatalFail);
            }

            string attachmentFilePath = Path.GetFullPath(downloadUrl);

            bool allowOverwrite = DetermineAllowOverwrite();
            return TryUploadAttachmentAsync(attachment, attachmentFilePath, allowOverwrite);
        }

        private string AssembleFileDownloadUrlFor(Attachment attachment, UserMessage referringUserMessage)
        {
            try
            {
                string extension = _repository.GetExtensionFromMimeType(attachment.ContentType);
                string fileName = PayloadFileNameFactory.CreateFileName(NamePattern, attachment, referringUserMessage);
                string validFileName = FilenameUtils.EnsureValidFilename($"{fileName}{extension}");

                return Path.Combine(Location, validFileName);
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

        private static async Task<UploadResult> TryUploadAttachmentAsync(Attachment attachment, string attachmentFilePath, bool allowOverwrite)
        {

            return await UploadAttachmentAsync(attachment, attachmentFilePath, allowOverwrite)
                   .ContinueWith(async t =>
                   {
                       if (t.IsFaulted)
                       {
                           IEnumerable<Exception> exs = t.Exception?.Flatten().InnerExceptions;
                           if (exs == null || exs.Any() == false)
                           {
                               return UploadResult.RetryableFail;
                           }

                           Exception unauthorizedEx = exs.FirstOrDefault(ex => ex is UnauthorizedAccessException);
                           if (unauthorizedEx != null)
                           {
                               Logger.Error(
                                   $"(Deliver) A fatal error occured while uploading the attachment {attachment.Id}: {unauthorizedEx.Message}");

                               return UploadResult.FatalFail;
                           }

                           // Filter IOExceptions on a specific HResult.
                           // -2147024816 is the HResult if the IOException is thrown because the file already exists.
                           Exception fileAlreadyExsitsEx =
                               exs.FirstOrDefault(ex => ex is IOException x && x.HResult == -2147024816);
                           if (fileAlreadyExsitsEx != null)
                           {
                               Logger.Error(
                                   $"(Deliver) Uploading file will be retried because a file already exists with the same name: {fileAlreadyExsitsEx}");

                               // If we happen to be in a concurrent scenario where there already
                               // exists a file with the same name, try to upload the file as well.
                               // The TryUploadAttachment method will generate a new name, but it is 
                               // still possible that, under heavy load, another file has been created
                               // with the same name as the unique name that we've generated.
                               // Therefore, retry again.
                               return await TryUploadAttachmentAsync(attachment, attachmentFilePath, allowOverwrite);
                           }

                           string desc = String.Join(", ", exs);
                           Logger.Error(
                               $"(Deliver) An error occured while uploading the attachment {attachment.Id}: {desc}, will be retried");

                           return UploadResult.RetryableFail;
                       }

                       if (t.IsCanceled)
                       {
                           return UploadResult.RetryableFail;
                       }

                       if (t.IsCompleted)
                       {
                           return t.Result;
                       }

                       return UploadResult.RetryableFail;
                   }).Unwrap();

        }

        private static async Task<UploadResult> UploadAttachmentAsync(Attachment attachment, string attachmentFilePath, bool overwriteExisting)
        {
            // Create the directory, if it does not exist.
            Directory.CreateDirectory(Path.GetDirectoryName(attachmentFilePath));

            (FileMode fileMode, string filePath) =
                overwriteExisting
                    ? (FileMode.Create, attachmentFilePath)
                    : (FileMode.CreateNew, FilenameUtils.EnsureFilenameIsUnique(attachmentFilePath)); 

            Logger.Trace($"Trying to upload attachment {attachment.Id} to {attachmentFilePath}");

            using (var fileStream = new FileStream(
                filePath, 
                fileMode, 
                FileAccess.Write, 
                FileShare.None, 
                bufferSize: 4096, 
                options: FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await attachment.Content.CopyToFastAsync(fileStream).ConfigureAwait(false);
            }

            Logger.Info($"(Deliver) Attachment {attachment.Id} is uploaded successfully to \"{attachmentFilePath}\"");
            return UploadResult.SuccessWithUrl(attachmentFilePath);
        }
    }
}