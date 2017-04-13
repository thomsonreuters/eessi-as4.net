using System;
using System.IO;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Utilities;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    /// <summary>
    /// <see cref="INotifySender"/> implementation to notify on the File Sytem.
    /// </summary>
    internal class FileNotifySender : INotifySender
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private Method _method;

        /// <summary>
        /// Configure the <see cref="INotifySender"/>
        /// with a given <paramref name="method"/>
        /// </summary>
        /// <param name="method"></param>
        public void Configure(Method method)
        {
            _method = method;
        }

        /// <summary>
        /// Send a given <paramref name="message"/> to a given endpoint
        /// </summary>
        /// <param name="message">The message.</param>
        public void Send(NotifyMessageEnvelope message)
        {
            string destinationUri = _method["location"].Value;
            TryCreateMissingDirectoriesIfNotExists(destinationUri);

            string locationFile = Path.Combine(
                destinationUri ?? string.Empty,
                FilenameSanitizer.EnsureValidFilename(message.MessageInfo.MessageId) + ".xml");

            SaveNotifyMessageFile(message, locationFile);
        }

        private static void TryCreateMissingDirectoriesIfNotExists(string locationFolder)
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

        private static void SaveNotifyMessageFile(NotifyMessageEnvelope notifyMessage, string locationFile)
        {
            using (FileStream fileStream = File.Create(locationFile))
            {
                fileStream.Write(notifyMessage.NotifyMessage,0, notifyMessage.NotifyMessage.Length);
                Logger.Info($"NotifyMessage {notifyMessage.MessageInfo.MessageId} is successfully send to: {locationFile}");
            }
        }
    }
   
}
