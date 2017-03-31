using System.IO;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Receivers;

namespace Eu.EDelivery.AS4.Model.Internal
{
    /// <summary>
    /// Canonical Return Message when the <see cref="IReceiver" />
    /// implementation has received a Message
    /// </summary>
    public class ReceivedMessage
    {
        protected ReceivedMessage() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedMessage" /> class.
        /// Create a new Received Message with a given RequestStream
        /// </summary>
        /// <param name="requestStream">
        /// </param>
        public ReceivedMessage(Stream requestStream)
        {
            RequestStream = requestStream;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedMessage" /> class.
        /// Create a new Received Message with a given RequestStream
        /// and Processing Mode
        /// </summary>
        /// <param name="requestStream">
        /// </param>
        /// <param name="contentType">
        /// </param>
        public ReceivedMessage(Stream requestStream, string contentType)
        {
            RequestStream = requestStream;
            ContentType = contentType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedMessage" /> class.
        /// Create a new Received Message with a given RequestStream
        /// and Processing Mode
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestStream">
        /// </param>
        /// <param name="contentType">
        /// </param>
        public ReceivedMessage(string id, Stream requestStream, string contentType)
        {
            RequestStream = requestStream;
            ContentType = contentType;
            Id = id;
        }

        public string Id { get; private set; }

        public string ContentType { get; set; }

        public Stream RequestStream { get; set; }

        /// <summary>
        /// Assign custom properties to the <see cref="ReceivedMessage" />
        /// </summary>
        /// <param name="message"></param>
        public virtual void AssignPropertiesTo(AS4Message message)
        {
            message.ContentType = ContentType;
        }
    }
}