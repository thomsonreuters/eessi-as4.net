using System;
using System.IO;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Utilities;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    /// <summary>
    /// Deliver the message to the file system
    /// </summary>
    [Info("FILE")]
    public class FileDeliverySender : IDeliverSender
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private Method _method;

        /// <summary>
        /// Configure the <see cref="IDeliverSender"/>
        /// with a given <paramref name="method"/>
        /// </summary>
        /// <param name="method"></param>
        public void Configure(Method method)
        {
            _method = method;
        }

        /// <summary>
        /// Start sending the <see cref="DeliverMessage"/>
        /// </summary>
        /// <param name="deliverMessage"></param>
        public void Send(DeliverMessageEnvelope deliverMessage)
        {
            string destinationUri = _method["location"].Value;
            TryCreateMissingDirectoriesIfNotExists(destinationUri);

            string filename = FilenameSanitizer.EnsureValidFilename(deliverMessage.MessageInfo.MessageId) + ".xml";
            string location = Path.Combine(destinationUri ?? string.Empty, filename);

            using (FileStream fileStream = File.Create(location))
            {
                fileStream.Write(deliverMessage.DeliverMessage, 0, deliverMessage.DeliverMessage.Length);

                Logger.Info($"DeliverMessage {deliverMessage.MessageInfo.MessageId} is successfully Send to: {location}");
            }
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
    }
}
