using System;
using System.IO;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.Utilities;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    /// <summary>
    /// Deliver the message to the file system
    /// </summary>
    [Info("FILE")]
    public class FileDeliverySender : DeliverySender
    {
        private readonly ILogger _logger;
       
        /// <summary>
        /// Initializes a new instance of the <see cref="FileDeliverySender"/> class. 
        /// Create a <see cref="IDeliverSender"/> implementation
        /// to send a <see cref="DeliverMessage"/>
        /// </summary>
        public FileDeliverySender()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        protected override void SendDeliverMessage(DeliverMessageEnvelope deliverMessage, string destinationUri)
        {
            TryCreateMissingDirectoriesIfNotExists(destinationUri);

            string filename = FilenameSanitizer.EnsureValidFilename(deliverMessage.MessageInfo.MessageId) + ".xml";
            string location = Path.Combine(destinationUri ?? "", filename);

            using (FileStream fileStream = File.Create(location))
            {
                fileStream.Write(deliverMessage.DeliverMessage, 0, deliverMessage.DeliverMessage.Length);

                this._logger.Info($"DeliverMessage {deliverMessage.MessageInfo.MessageId} is successfully Send to: {location}");
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
                this.Log.Error(exception.Message);
            }
        }

    }
}
