using System;
using System.IO;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.Utilities;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    /// <summary>
    /// Deliver the message to the file system
    /// </summary>
    [Info("FILE")]
    public class FileDeliverXmlSender : IDeliverSender
    {
        private readonly ILogger _logger;

        [Info("Location")]
        public Parameter LocationParameter { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDeliverXmlSender"/> class. 
        /// Create a <see cref="IDeliverSender"/> implementation
        /// to send a <see cref="DeliverMessage"/>
        /// </summary>
        public FileDeliverXmlSender()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Configure the <see cref="IDeliverSender"/>
        /// with a given <paramref name="method"/>
        /// </summary>
        /// <param name="method"></param>
        public void Configure(Method method)
        {
            this.LocationParameter = method["location"];
        }

        /// <summary>
        /// Start sending the <see cref="DeliverMessage"/>
        /// </summary>
        /// <param name="deliverMessage"></param>
        public void Send(DeliverMessage deliverMessage)
        {            
            TrySendDeliverMessage(deliverMessage, LocationParameter.Value);
        }

        private void TrySendDeliverMessage(DeliverMessage deliverMessage, string locationFolder)
        {
            try
            {
                SendDeliverMessage(deliverMessage, locationFolder);
            }
            catch (SystemException)
            {
                throw ThrowAS4DeliverSendException(deliverMessage.MessageInfo.MessageId, locationFolder);
            }
        }

        private void SendDeliverMessage(DeliverMessage deliverMessage, string locationFolder)
        {

            string filename = FilenameSanitizer.EnsureValidFilename(deliverMessage.MessageInfo.MessageId) + ".xml";
            string location = Path.Combine(locationFolder, filename);

            using (FileStream fileStream = File.Create(location))
            {
                var serializer = new XmlSerializer(typeof(DeliverMessage));
                serializer.Serialize(fileStream, deliverMessage);
                this._logger.Info($"DeliverMessage {deliverMessage.MessageInfo.MessageId} is successfully Send to: {location}");
            }
        }

        private AS4Exception ThrowAS4DeliverSendException(string messageId, string location)
        {
            string description = $"Unable to serialize DeliverMessage {messageId} to a Xml stream at location: {location}";
            this._logger.Error(description);

            return new AS4Exception(description);
        }
    }
}
