using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Security;
using NLog;

using Function =
    System.Func
    <Eu.EDelivery.AS4.Model.Internal.ReceivedMessage, System.Threading.CancellationToken,
        System.Threading.Tasks.Task<Eu.EDelivery.AS4.Model.Internal.InternalMessage>>;

namespace Eu.EDelivery.AS4.Receivers
{
    /// <summary>
    /// <see cref="IReceiver" /> Implementation to receive Files
    /// </summary>
    [Info("File receiver")]
    public class FileReceiver : PollingTemplate<FileInfo, ReceivedMessage>, IReceiver
    {
        private readonly IMimeTypeRepository _repository;
        private IDictionary<string, string> _properties;
        [Info("File path")]
        [Description("Path to the folder to poll for new files")]
        private string FilePath => this._properties.ReadMandatoryProperty("FilePath");
        [Info("File mask")]
        private string FileMask => this._properties.ReadOptionalProperty("FileMask", "*.*");
        [Info("Username")]
        private string Username => this._properties.ReadOptionalProperty("Username");
        [Info("Password")]
        private string Password => this._properties.ReadOptionalProperty("Password");
        protected override ILogger Logger { get; }

        [Info("Polling interval", "", "int")]
        protected override TimeSpan PollingInterval => FromProperties();

        /// <summary>
        /// Initializes a new instance of the <see cref="FileReceiver"/> class
        /// </summary>
        public FileReceiver()
        {
            this._repository = new MimeTypeRepository();
            this.Logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Configure File Receiver
        /// </summary>
        /// <param name="properties"></param>
        public void Configure(IDictionary<string, string> properties)
        {
            this._properties = properties;
        }

        /// <summary>
        /// Start Receiving on the given File Location
        /// </summary>
        /// <param name="messageCallback"></param>
        /// <param name="cancellationToken"></param>
        public void StartReceiving(Function messageCallback, CancellationToken cancellationToken)
        {
            this.Logger.Info($"Start receiving on '{Path.GetFullPath(this.FilePath)}'...");
            StartPolling(messageCallback, cancellationToken);
        }

        /// <summary>
        /// Declaration to where the Message are and can be polled
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override IEnumerable<FileInfo> GetMessagesToPoll(CancellationToken cancellationToken)
        {
            var directoryInfo = new DirectoryInfo(this.FilePath);
            var fileNames = new FileInfo[0];
            WithImpersonation(() => fileNames = directoryInfo.GetFiles(this.FileMask));

            return fileNames;
        }

        /// <summary>
        /// Declaration to the action that has to executed when a Message is received
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="messageCallback">Message Callback after the Message is received</param>
        /// <param name="token"></param>
        protected override void MessageReceived(FileInfo entity, Function messageCallback, CancellationToken token)
        {
            this.Logger.Info($"Received Message from Filesystem: {entity.Name}");
            WithImpersonation(() => GetFileFromMessage(entity, messageCallback, token));
        }

        /// <summary>
        /// Describe what to do in case of an Exception
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="exception"></param>
        protected override void HandleMessageException(FileInfo fileInfo, Exception exception)
        {
            this.Logger.Error(exception.Message);
            MoveFile(fileInfo, "exception");
        }

        private void GetFileFromMessage(FileInfo fileInfo, Function messageCallback, CancellationToken token)
        {
            if (!fileInfo.Exists) return;
            this.Logger.Info($"Received file '{fileInfo.Name}'");

            OpenStreamFromMessage(fileInfo, messageCallback, token);
        }

        private async void OpenStreamFromMessage(FileInfo fileInfo, Function messageCallback, CancellationToken token)
        {
            string contentType = this._repository.GetMimeTypeFromExtension(fileInfo.Extension);

            MoveFile(fileInfo, "processing");

            InternalMessage internalMessage;
            using (Stream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                var receivedMessage = new ReceivedMessage(fileStream, contentType);
                internalMessage = await messageCallback(receivedMessage, token);
            }

            NotifyReceivedFile(fileInfo, internalMessage);
        }

        private void NotifyReceivedFile(FileInfo fileInfo, InternalMessage internalMessage)
        {
            if (internalMessage.Exception != null)
                HandleException(fileInfo, internalMessage.Exception);

            else MoveFile(fileInfo, "accepted");
        }

        private void HandleException(FileInfo fileInfo, AS4Exception as4Exception)
        {
            MoveFile(fileInfo, "exception");
            CreateExceptionFile(fileInfo, as4Exception);
        }

        private void CreateExceptionFile(FileSystemInfo fileInfo, AS4Exception as4Exception)
        {
            string fileName = fileInfo.FullName + ".details";
            this.Logger.Info($"Exception Details are stored at: {fileName}");

            using (var streamWriter = new StreamWriter(fileName))
                streamWriter.WriteLine(as4Exception.ToString());
        }

        /// <summary>
        /// Move file to another place on the File System
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="extension"></param>
        private void MoveFile(FileInfo fileInfo, string extension)
        {
            this.Logger.Debug($"Renaming file '{fileInfo.Name}'...");
            string destFileName = $"{fileInfo.Directory?.FullName}\\{Path.GetFileNameWithoutExtension(fileInfo.FullName)}.{extension}";

            if (File.Exists(destFileName))
            {
                const string copyExtension = " - Copy";
                string copyFilename = destFileName + copyExtension;
                this.Logger.Debug($"File with name: {destFileName} already exists, renaming to {copyFilename}");
                MoveFile(fileInfo, extension + copyExtension);
            }
            else fileInfo.MoveTo(destFileName);

            this.Logger.Info($"File renamed to: '{fileInfo.Name}'!");
        }

        private TimeSpan FromProperties()
        {
            string pollingInterval = this._properties.ReadMandatoryProperty("PollingInterval");
            double miliseconds = Convert.ToDouble(pollingInterval);

            return TimeSpan.FromMilliseconds(miliseconds);
        }

        private void WithImpersonation(Action action)
        {
            object impersonationContext = null;

            try
            {
                impersonationContext = ImpersonateWhenRequired();
                action();
            }
            finally
            {
                if (impersonationContext != null)
                    Impersonation.UndoImpersonation(impersonationContext);
            }
        }

        private object ImpersonateWhenRequired()
        {
            if (string.IsNullOrEmpty(this.Username))
                return null;

            this.Logger.Trace($"Impersonating as user {this.Username}");
            return Impersonation.Impersonate(this.Username, this.Password);
        }
    }
}