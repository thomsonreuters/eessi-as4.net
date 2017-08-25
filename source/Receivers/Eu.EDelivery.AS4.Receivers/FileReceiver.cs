using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using NLog;
using Function =
    System.Func<Eu.EDelivery.AS4.Model.Internal.ReceivedMessage, System.Threading.CancellationToken,
        System.Threading.Tasks.Task<Eu.EDelivery.AS4.Model.Internal.MessagingContext>>;

namespace Eu.EDelivery.AS4.Receivers
{
    /// <summary>
    /// <see cref="IReceiver" /> Implementation to receive Files
    /// </summary>
    [Info("FILE receiver")]
    public class FileReceiver : PollingTemplate<FileInfo, ReceivedMessage>, IReceiver
    {
        private readonly SynchronizedCollection<FileInfo> _pendingFiles = new SynchronizedCollection<FileInfo>();
        private readonly IMimeTypeRepository _repository;

        private bool _isReceiving = false;
        private FileReceiverSettings _settings;
        /// <summary>
        /// Initializes a new instance of the <see cref="FileReceiver" /> class
        /// </summary>
        public FileReceiver()
        {
            _repository = new MimeTypeRepository();
            Logger = LogManager.GetCurrentClassLogger();
        }

        [Info("File path", required: true)]
        [Description("Path to the folder to poll for new files")]
        private string FilePath => _settings.FilePath;

        [Info("File mask", required: true, defaultValue: "*.*")]
        [Description("Mask used to match files.")]
        private string FileMask => _settings.FileMask;

        [Info("Batch size", required: true, defaultValue: SettingKeys.BatchSizeDefault)]
        [Description("Indicates how many files should be processed per batch.")]
        private int BatchSize => _settings.BatchSize;

        [Info("Polling interval", defaultValue: SettingKeys.PollingIntervalDefault)]
        protected override TimeSpan PollingInterval => _settings.PollingInterval;

        protected override ILogger Logger { get; }

        private static readonly string[] ExcludedExtensions = { ".pending", ".processing", ".accepted", ".exception", ".details" };

        #region Configuration

        private static class SettingKeys
        {
            public const string FilePath = "FilePath";
            public const string FileMask = "FileMask";
            public const string BatchSize = "BatchSize";
            public const string BatchSizeDefault = "20";
            public const string PollingInterval = "PollingInterval";
            public const string PollingIntervalDefault = "00:00:03";
        }

        public void Configure(FileReceiverSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Configure the receiver with a given settings dictionary.
        /// </summary>
        /// <param name="settings"></param>
        void IReceiver.Configure(IEnumerable<Setting> settings)
        {
            var properties = settings.ToDictionary(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase);

            var configuredBatchSize = properties.ReadOptionalProperty(SettingKeys.BatchSize, SettingKeys.BatchSizeDefault);

            if (Int32.TryParse(configuredBatchSize, out var batchSize) == false)
            {
                batchSize = 20;
            }

            _settings = new FileReceiverSettings(properties.ReadMandatoryProperty(SettingKeys.FilePath),
                                                 properties.ReadOptionalProperty(SettingKeys.FileMask, "*.*"),
                                                 batchSize,
                                                 ReadPollingIntervalFromProperties(properties));
        }

        #endregion

        /// <summary>
        /// Start Receiving on the given File LocationParameter
        /// </summary>
        /// <param name="messageCallback"></param>
        /// <param name="cancellationToken"></param>
        public void StartReceiving(Function messageCallback, CancellationToken cancellationToken)
        {
            _isReceiving = true;
            Logger.Debug($"Start receiving on '{Path.GetFullPath(FilePath)}'...");
            StartPolling(messageCallback, cancellationToken);
        }

        public void StopReceiving()
        {
            _isReceiving = false;
            Logger.Debug($"Stop receiving on '{Path.GetFullPath(FilePath)}'...");
        }

        private async void GetMessageFromFile(FileInfo fileInfo, Function messageCallback, CancellationToken token)
        {
            if (!fileInfo.Exists)
            {
                return;
            }

            Logger.Info($"Getting Message from file '{fileInfo.Name}'");

            await OpenStreamFromMessage(fileInfo, messageCallback, token);

            _pendingFiles.Remove(fileInfo);
        }

        private async Task OpenStreamFromMessage(FileInfo fileInfo, Function messageCallback, CancellationToken token)
        {
            try
            {
                string contentType = _repository.GetMimeTypeFromExtension(fileInfo.Extension);

                var result = MoveFile(fileInfo, "processing");

                if (result.success)
                {
                    MessagingContext messagingContext = null;

                    try
                    {
                        using (Stream fileStream = new FileStream(result.filename, FileMode.Open, FileAccess.Read))
                        {
                            fileStream.Seek(0, SeekOrigin.Begin);
                            var receivedMessage = new ReceivedMessage(fileStream, contentType);
                            messagingContext = await messageCallback(receivedMessage, token).ConfigureAwait(false);
                        }

                        await NotifyReceivedFile(fileInfo, messagingContext).ConfigureAwait(false);
                    }
                    finally
                    {
                        messagingContext?.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occured while processing {fileInfo.Name}");
                Logger.Trace(ex.Message);
            }
        }

        private async Task NotifyReceivedFile(FileInfo fileInfo, MessagingContext messagingContext)
        {
            if (messagingContext.Exception != null)
            {
                await HandleException(fileInfo, messagingContext.Exception);
            }
            else
            {
                MoveFile(fileInfo, "accepted");
            }
        }

        private async Task HandleException(FileInfo fileInfo, Exception exception)
        {
            MoveFile(fileInfo, "exception");
            await CreateExceptionFile(fileInfo, exception);
        }

        private async Task CreateExceptionFile(FileSystemInfo fileInfo, Exception exception)
        {
            string fileName = fileInfo.FullName + ".details";
            Logger.Info($"Exception Details are stored at: {fileName}");

            using (var streamWriter = new StreamWriter(fileName))
            {
                await streamWriter.WriteLineAsync(exception.ToString()).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Move file to another place on the File System
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="extension"></param>
        private (bool success, string filename) MoveFile(FileInfo fileInfo, string extension)
        {
            extension = extension.TrimStart('.');

            Logger.Debug($"Renaming file '{fileInfo.Name}'...");
            string destFileName =
                $"{fileInfo.Directory?.FullName}\\{Path.GetFileNameWithoutExtension(fileInfo.FullName)}.{extension}";

            try
            {
                destFileName = EnsureFilenameIsUnique(destFileName);

                int attempts = 0;

                do
                {
                    try
                    {
                        fileInfo.MoveTo(destFileName);
                        attempts = 5;
                    }
                    catch (IOException)
                    {
                        // When the file is in use, an IO exception will be thrown.
                        // If that is the case, wait a little and retry.                       
                        if (attempts == 5)
                        {
                            throw;
                        }
                        attempts++;
                        Thread.Sleep(500);
                    }
                } while (attempts < 5);

                Logger.Info($"File renamed to: '{fileInfo.Name}'!");

                return (success: true, filename: destFileName);
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to MoveFile {fileInfo.FullName} to {destFileName}");
                Logger.Error(ex.Message);
                Logger.Trace(ex.StackTrace);
                return (success: false, filename: string.Empty);
            }
        }

        private static string EnsureFilenameIsUnique(string filename)
        {
            while (File.Exists(filename))
            {
                const string copyExtension = " - Copy";

                string name = Path.GetFileName(filename) + copyExtension + Path.GetExtension(filename);
                string copyFilename = Path.Combine(Path.GetDirectoryName(filename) ?? string.Empty, name);

                filename = copyFilename;
            }

            return filename;
        }

        private static TimeSpan ReadPollingIntervalFromProperties(Dictionary<string, string> properties)
        {
            if (properties.ContainsKey(SettingKeys.PollingInterval) == false)
            {
                return TimeSpan.Parse(SettingKeys.PollingIntervalDefault);
            }

            string pollingInterval = properties[SettingKeys.PollingInterval];
            return pollingInterval.AsTimeSpan(TimeSpan.Parse(SettingKeys.PollingIntervalDefault));
        }

        /// <summary>
        /// Declaration to where the Message are and can be polled
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override IEnumerable<FileInfo> GetMessagesToPoll(CancellationToken cancellationToken)
        {
            var directoryInfo = new DirectoryInfo(FilePath);
            var resultedFiles = new List<FileInfo>();

            if (cancellationToken.IsCancellationRequested || _isReceiving == false)
            {
                return new FileInfo[] { };
            }

            FileInfo[] directoryFiles =
                directoryInfo.GetFiles(FileMask)
                             .Where(fi => ExcludedExtensions.Contains(fi.Extension) == false)
                             .Take(BatchSize).ToArray();

            foreach (FileInfo file in directoryFiles)
            {
                try
                {
                    var result = MoveFile(file, "pending");

                    if (result.success)
                    {
                        var pendingFile = new FileInfo(result.filename);

                        Logger.Trace(
                            $"Locked file {file.Name} to be processed and renamed it to {pendingFile.Name}");

                        _pendingFiles.Add(pendingFile);

                        resultedFiles.Add(pendingFile);
                    }
                }
                catch (IOException ex)
                {
                    Logger.Info($"FileReceiver on {FilePath}: {file.Name} skipped since it is in use.");
                    Logger.Trace(ex.Message);
                }
            }

            return resultedFiles;
        }

        protected override void ReleasePendingItems()
        {
            // Rename the 'pending' files to their original filename.
            string extension = Path.GetExtension(FileMask);

            lock (_pendingFiles.SyncRoot)
            {
                for (int i = _pendingFiles.Count - 1; i >= 0; i--)
                {
                    var pendingFile = _pendingFiles[i];

                    if (File.Exists(pendingFile.FullName))
                    {
                        MoveFile(pendingFile, extension);
                    }
                    _pendingFiles.Remove(pendingFile);
                }
            }
        }

        /// <summary>
        /// Declaration to the action that has to executed when a Message is received
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="messageCallback">Message Callback after the Message is received</param>
        /// <param name="token"></param>
        protected override void MessageReceived(FileInfo entity, Function messageCallback, CancellationToken token)
        {
            Logger.Info($"Received Message from Filesystem: {entity.Name}");
            GetMessageFromFile(entity, messageCallback, token);
        }

        /// <summary>
        /// Describe what to do in case of an Exception
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="exception"></param>
        protected override void HandleMessageException(FileInfo fileInfo, Exception exception)
        {
            Logger.Error(exception.Message);
            MoveFile(fileInfo, "exception");
        }
    }
}