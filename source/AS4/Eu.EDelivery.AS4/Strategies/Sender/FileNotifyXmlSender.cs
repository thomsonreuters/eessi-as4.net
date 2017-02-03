using System;
using System.IO;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Utilities;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    /// <summary>
    /// <see cref="INotifySender"/> implementation 
    /// to send the <see cref="NotifyMessage"/> to the file system
    /// </summary>
    public class FileNotifyXmlSender : INotifySender
    {
        private readonly ILogger _logger;
        private Method _method;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileNotifyXmlSender"/> class. 
        /// Create a <see cref="INotifySender"/> implementation
        /// to send a <see cref="NotifyMessage"/>
        /// </summary>
        public FileNotifyXmlSender()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Configure the <see cref="INotifySender"/>
        /// with a given <paramref name="method"/>
        /// </summary>
        /// <param name="method"></param>
        public void Configure(Method method)
        {
            this._method = method;
        }

        /// <summary>
        /// Start sending the <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="notifyMessage"></param>
        public void Send(NotifyMessage notifyMessage)
        {
            Parameter locationParameter = this._method["location"];
            TrySendNotifyMessage(notifyMessage, locationParameter.Value);
        }

        private void TrySendNotifyMessage(NotifyMessage notifyMessage, string locationFolder)
        {
            try
            {
                SendNotifyMessage(notifyMessage, locationFolder);
            }
            catch (InvalidOperationException)
            {
                throw ThrowAS4NotifySendException(notifyMessage.MessageInfo.MessageId, locationFolder);
            }
        }

        private void SendNotifyMessage(NotifyMessage notifyMessage, string locationFolder)
        {
            string locationFile = Path.Combine(locationFolder ?? "",
                                               FilenameSanitizer.EnsureValidFilename(notifyMessage.MessageInfo.MessageId) + ".xml");

            TryCreateMissingDirectoriesIfNotExists(locationFolder);
            TrySendNotifyMessageFile(notifyMessage, locationFile);
        }

        private void TrySendNotifyMessageFile(NotifyMessage notifyMessage, string locationFile)
        {
            try
            {
                SendNotifyMessageFile(notifyMessage, locationFile);
            }
            catch (Exception exception)
            {
                this._logger.Error(exception.Message);
            }
        }

        private void SendNotifyMessageFile(NotifyMessage notifyMessage, string locationFile)
        {
            using (FileStream fileStream = File.Create(locationFile))
            {
                var serializer = new XmlSerializer(typeof(NotifyMessage));
                serializer.Serialize(fileStream, notifyMessage);
                this._logger.Info($"NotifyMessage {notifyMessage.MessageInfo.MessageId} is successfully send to: {locationFile}");
            }
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
                this._logger.Error(exception.Message);
            }
        }

        private AS4Exception ThrowAS4NotifySendException(string messageId, string location)
        {
            string description = $"Unable to serialize NotifyMessage {messageId} to a Xml stream at location: {location}";
            this._logger.Error(description);

            return new AS4Exception(description);
        }
    }
}
