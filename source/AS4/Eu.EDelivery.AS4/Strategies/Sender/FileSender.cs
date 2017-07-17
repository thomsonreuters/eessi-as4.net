using System;
using System.IO;
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
    [Info("FILE")]
    public class FileSender : IDeliverSender, INotifySender
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private string _destinationPath;
        [Info("Destination path")]
        private string DestinationPath { get; set; }

        /// <summary>
        /// Configure the <see cref="INotifySender"/>
        /// with a given <paramref name="method"/>
        /// </summary>
        /// <param name="method"></param>
        public void Configure(Method method)
        {
            _destinationPath = method["location"].Value;
        }

        /// <summary>
        /// Start sending the <see cref="DeliverMessage"/>
        /// </summary>
        /// <param name="deliverMessage"></param>
        public async Task SendAsync(DeliverMessageEnvelope deliverMessage)
        {
            EnsureDirectory(_destinationPath);

            string location = CombineDestinationFullName(deliverMessage.MessageInfo.MessageId, _destinationPath);
            await WriteContentsToFile(location, deliverMessage.DeliverMessage).ConfigureAwait(false);

            Logger.Info($"DeliverMessage {deliverMessage.MessageInfo.MessageId} is successfully Send to: {location}");
        }

        /// <summary>
        /// Start sending the <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="notifyMessage"></param>
        public async Task SendAsync(NotifyMessageEnvelope notifyMessage)
        {
            EnsureDirectory(_destinationPath);

            string location = CombineDestinationFullName(notifyMessage.MessageInfo.MessageId, _destinationPath);
            await WriteContentsToFile(location, notifyMessage.NotifyMessage).ConfigureAwait(false);

            Logger.Info($"NotifyMessage {notifyMessage.MessageInfo.MessageId} is successfully Send to: {location}");
        }

        private static void EnsureDirectory(string locationFolder)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(locationFolder) && !Directory.Exists(locationFolder))
                {
                    Directory.CreateDirectory(locationFolder);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
            }
        }

        private static string CombineDestinationFullName(string fileName, string destinationFolder)
        {
            string filename = FilenameSanitizer.EnsureValidFilename(fileName) + ".xml";
            return Path.Combine(destinationFolder ?? string.Empty, filename);
        }

        private static async Task WriteContentsToFile(string locationPath, byte[] contents)
        {
            using (FileStream fileStream = File.Create(locationPath))
            {
                await fileStream.WriteAsync(contents, 0, contents.Length).ConfigureAwait(false);
            }
        }
    }
}
