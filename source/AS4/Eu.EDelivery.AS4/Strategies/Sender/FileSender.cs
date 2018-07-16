using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Utilities;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    /// <summary>
    /// <see cref="IDeliverSender"/>, <see cref="INotifySender"/> implementation to write contensts to the File System.
    /// </summary>
    [Info(FileSender.Key)]
    public class FileSender : IDeliverSender, INotifySender
    {
        public const string Key = "FILE";

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        [Info("Destination path")]
        private string Location { get; set; }

        /// <summary>
        /// Configure the <see cref="INotifySender"/>
        /// with a given <paramref name="method"/>
        /// </summary>
        /// <param name="method"></param>
        public void Configure(Method method)
        {
            Location = method["location"].Value;
        }

        /// <summary>
        /// Start sending the <see cref="DeliverMessage"/>
        /// </summary>
        /// <param name="deliverMessage"></param>
        public async Task<SendResult> SendAsync(DeliverMessageEnvelope deliverMessage)
        {
            SendResult directoryResult = EnsureDirectory(Location);
            if (directoryResult == SendResult.FatalFail)
            {
                return directoryResult;
            }

            string location = CombineDestinationFullName(deliverMessage.MessageInfo.MessageId, Location);
            Logger.Trace($"Sending DeliverMessage to {location}");

            SendResult result = await TryWriteContentsToFileAsync(location, deliverMessage.DeliverMessage);
            if (result == SendResult.Success)
            {
                Logger.Info(
                    $"(Deliver) DeliverMessage {deliverMessage.MessageInfo.MessageId} is successfully send to \"{location}\"");
            }

            return result;
        }

        /// <summary>
        /// Start sending the <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="notifyMessage"></param>
        public async Task<SendResult> SendAsync(NotifyMessageEnvelope notifyMessage)
        {
            SendResult directoryResult = EnsureDirectory(Location);
            if (directoryResult == SendResult.FatalFail)
            {
                return directoryResult;
            }

            string location = CombineDestinationFullName(notifyMessage.MessageInfo.MessageId, Location);
            Logger.Trace($"Sending NotifyMessage to {location}");

            SendResult result = await TryWriteContentsToFileAsync(location, notifyMessage.NotifyMessage);
            if (result == SendResult.Success)
            {
                Logger.Info(
                    $"(Notify) NotifyMessage {notifyMessage.MessageInfo.MessageId} is successfully send to \"{location}\"");
            }

            return result;
        }

        private static string CombineDestinationFullName(string fileName, string destinationFolder)
        {
            string filename = FilenameUtils.EnsureValidFilename(fileName) + ".xml";
            return Path.Combine(destinationFolder ?? string.Empty, filename);
        }

        private static SendResult EnsureDirectory(string locationFolder)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(locationFolder) && !Directory.Exists(locationFolder))
                {
                    Directory.CreateDirectory(locationFolder);
                }

                return SendResult.Success;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return SendResult.FatalFail;
            }
        }

        private static Task<SendResult> TryWriteContentsToFileAsync(string locationPath, byte[] contents)
        {
            return WriteContentsToFileAsync(locationPath, contents)
                .ContinueWith(async t =>
                   {
                       if (t.IsFaulted)
                       {
                           IEnumerable<Exception> exs = t.Exception?.Flatten().InnerExceptions;
                           if (exs == null || exs.Any() == false)
                           {
                               return SendResult.RetryableFail;
                           }

                           Exception unauthorizedEx = exs.FirstOrDefault(ex => ex is UnauthorizedAccessException);
                           if (unauthorizedEx != null)
                           {
                               Logger.Error(
                                   $"A fatal error occured while uploading the file to \"{locationPath}\": {unauthorizedEx.Message}");

                               return SendResult.FatalFail;
                           }

                           // Filter IOExceptions on a specific HResult.
                           // -2147024816 is the HResult if the IOException is thrown because the file already exists.
                           Exception fileAlreadyExsitsEx =
                               exs.FirstOrDefault(ex => ex is IOException x && x.HResult == -2147024816);
                           if (fileAlreadyExsitsEx != null)
                           {
                               Logger.Error(
                                   "(Deliver) Uploading file will be retried "
                                   + $"because a file already exists with the same name: {fileAlreadyExsitsEx}");

                               // If we happen to be in a concurrent scenario where there already
                               // exists a file with the same name, try to upload the file as well.
                               // The TryUploadAttachment method will generate a new name, but it is 
                               // still possible that, under heavy load, another file has been created
                               // with the same name as the unique name that we've generated.
                               // Therefore, retry again.
                               return await TryWriteContentsToFileAsync(locationPath, contents);
                           }

                           string desc = String.Join(", ", exs);
                           Logger.Error(
                               $"An error occured while uploading the file to \"{locationPath}\": {desc}, will be retried");

                           return SendResult.RetryableFail;

                       }

                       if (t.IsCanceled)
                       {
                           return SendResult.RetryableFail;
                       }

                       if (t.IsCompleted)
                       {
                           return t.Result;
                       }

                       return SendResult.RetryableFail;
                   }).Unwrap();
        }

        private static async Task<SendResult> WriteContentsToFileAsync(string locationPath, byte[] contents)
        {
            using (FileStream fileStream = FileUtils.CreateAsync(locationPath, FileOptions.SequentialScan))
            {
                await fileStream.WriteAsync(contents, 0, contents.Length);

                return SendResult.Success;
            }
        }
    }
}
