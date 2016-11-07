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
        public string ContentType { get; set; }
        public Stream RequestStream { get; set; }

        protected ReceivedMessage() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedMessage"/> class. 
        /// Create a new Received Message with a given RequestStream
        /// </summary>
        /// <param name="requestStream">
        /// </param>
        public ReceivedMessage(Stream requestStream)
        {
            this.RequestStream = requestStream;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedMessage"/> class. 
        /// Create a new Received Message with a given RequestStream
        /// and Processing Mode
        /// </summary>
        /// <param name="requestStream">
        /// </param>
        /// <param name="contentType">
        /// </param>
        public ReceivedMessage(Stream requestStream, string contentType)
        {
            this.RequestStream = requestStream;
            this.ContentType = contentType;
        }

        /// <summary>
        /// Assign custom properties to the <see cref="ReceivedMessage"/>
        /// </summary>
        /// <param name="message"></param>
        public virtual void AssignProperties(AS4Message message)
        {
            message.ContentType = this.ContentType;
        }
    }
}