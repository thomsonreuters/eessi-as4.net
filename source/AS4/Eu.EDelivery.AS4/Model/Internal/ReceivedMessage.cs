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
        protected ReceivedMessage() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedMessage" /> class.
        /// Create a new Received Message with a given RequestStream
        /// </summary>
        /// <param name="underlyingStream">
        /// </param>
        public ReceivedMessage(Stream underlyingStream)
        {
            UnderlyingStream = underlyingStream;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedMessage" /> class.
        /// Create a new Received Message with a given RequestStream
        /// and Processing Mode
        /// </summary>
        /// <param name="underlyingStream">
        /// </param>
        /// <param name="contentType">
        /// </param>
        public ReceivedMessage(Stream underlyingStream, string contentType)
        {
            UnderlyingStream = underlyingStream;
            ContentType = contentType;
        }

        public string ContentType { get; private set; }

        public Stream UnderlyingStream { get; private set; }

        /// <summary>
        /// Assign custom properties to the <see cref="ReceivedMessage" />
        /// </summary>
        /// <param name="messagingContext"></param>
        public virtual void AssignPropertiesTo(MessagingContext messagingContext)
        {
            if (messagingContext.AS4Message != null)
            {
                messagingContext.AS4Message.ContentType = ContentType;
            }
        }
    }
}