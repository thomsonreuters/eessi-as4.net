using System;
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
            if (directoryResult.Status == SendStatus.Fail)
            {
                return directoryResult;
            }

            string location = CombineDestinationFullName(deliverMessage.MessageInfo.MessageId, Location);
            Logger.Trace($"(Deliver) Sending DeliverMessage to {location}");

            return await WriteContentsToFile(location, deliverMessage.DeliverMessage)
                .ContinueWith(t =>
                {
                    if (t.Result.Status == SendStatus.Success)
                    {
                        Logger.Info(
                            $"(Deliver) DeliverMessage {deliverMessage.MessageInfo.MessageId} " +
                            $"is successfully send to {location}");
                    }

                    return t.Result;
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        /// <summary>
        /// Start sending the <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="notifyMessage"></param>
        public async Task<SendResult> SendAsync(NotifyMessageEnvelope notifyMessage)
        {
            EnsureDirectory(Location);

            string location = CombineDestinationFullName(notifyMessage.MessageInfo.MessageId, Location);

            Logger.Trace($"(Notify) Sending NotifyMessage to {location}");

            return await WriteContentsToFile(location, notifyMessage.NotifyMessage)
                .ContinueWith(t =>
                {
                    if (t.Result.Status == SendStatus.Success)
                    {
                        Logger.Info(
                            $"(Notify) NotifyMessage {notifyMessage.MessageInfo.MessageId} " +
                            $"is successfully send to {location}");
                    }

                    return t.Result;
                }, TaskContinuationOptions.NotOnRanToCompletion);
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

        private static Task<SendResult> WriteContentsToFile(string locationPath, byte[] contents)
        {
            try
            {
                using (FileStream fileStream = FileUtils.CreateAsync(locationPath, FileOptions.SequentialScan))
                {
                    return fileStream
                        .WriteAsync(contents, 0, contents.Length)
                        .ContinueWith(t => SendResult.Success, 
                                      TaskContinuationOptions.OnlyOnRanToCompletion);
                }
            }
            catch (AggregateException ex)
            {
                Logger.Error(ex);

                bool containsAnyUnauthorizedExceptions =
                    ex.Flatten().InnerExceptions.Any(e => e is UnauthorizedAccessException);

                return Task.FromResult(
                    containsAnyUnauthorizedExceptions
                        ? SendResult.FatalFail
                        : SendResult.RetryableFail);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                return Task.FromResult(
                    SendResult.RetryableFail);
            }
        }
    }
}
