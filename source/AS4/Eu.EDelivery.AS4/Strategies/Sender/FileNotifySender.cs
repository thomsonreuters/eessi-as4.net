using System;
using System.IO;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Utilities;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    internal class FileNotifySender : SenderNotifier
    {
        protected override void SendNotifyMessage(NotifyMessageEnvelope notifyMessage, string destinationUri)
        {
            TryCreateMissingDirectoriesIfNotExists(destinationUri);

            string locationFile = Path.Combine(destinationUri ?? "",
                                               FilenameSanitizer.EnsureValidFilename(notifyMessage.MessageInfo.MessageId) + ".xml");

            SaveNotifyMessageFile(notifyMessage, locationFile);
        }

        private void TryCreateMissingDirectoriesIfNotExists(string locationFolder)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(locationFolder) && !Directory.Exists(locationFolder))
                {
                    Directory.CreateDirectory(locationFolder);
                }
            }
            catch (Exception exception)
            {
                this.Log.Error(exception.Message);
            }
        }

        private void SaveNotifyMessageFile(NotifyMessageEnvelope notifyMessage, string locationFile)
        {
            using (FileStream fileStream = File.Create(locationFile))
            {
                fileStream.Write(notifyMessage.NotifyMessage,0, notifyMessage.NotifyMessage.Length);                
                this.Log.Info($"NotifyMessage {notifyMessage.MessageInfo.MessageId} is successfully send to: {locationFile}");
            }
        }
    }
   
}
