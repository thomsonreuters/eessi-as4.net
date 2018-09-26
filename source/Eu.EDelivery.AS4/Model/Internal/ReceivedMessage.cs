using System;
using System.IO;

namespace Eu.EDelivery.AS4.Model.Internal
{
    /// <summary>
    /// Canonical Return Message when the <see cref="Receivers.IReceiver" />  implementation has received a Message
    /// </summary>
    public class ReceivedMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedMessage" /> class.
        /// </summary>
        /// <param name="underlyingStream"> </param>
        public ReceivedMessage(Stream underlyingStream) 
            : this(underlyingStream, "application/octet-stream") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedMessage" /> class.
        /// and Processing Mode
        /// </summary>
        /// <param name="underlyingStream"> </param>
        /// <param name="contentType"> </param>
        public ReceivedMessage(Stream underlyingStream, string contentType) 
            : this(underlyingStream, contentType, origin: "unknown origin") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedMessage"/> class.
        /// </summary>
        /// <param name="underlyingStream"></param>
        /// <param name="contentType"></param>
        /// <param name="origin"></param>
        public ReceivedMessage(Stream underlyingStream, string contentType, string origin) 
            : this(underlyingStream, contentType, origin, length: -1) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedMessage"/> class.
        /// </summary>
        /// <param name="underlyingStream"></param>
        /// <param name="contentType"></param>
        /// <param name="origin"></param>
        /// <param name="length"></param>
        public ReceivedMessage(
            Stream underlyingStream,
            string contentType,
            string origin,
            long length)
        {
            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            if (underlyingStream == null)
            {
                throw new ArgumentNullException(nameof(underlyingStream));
            }

            if (origin == null)
            {
                throw new ArgumentNullException(nameof(origin));
            }

            if (length < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            ContentType = contentType;
            UnderlyingStream = underlyingStream;
            Origin = origin;
            Length = length;
        }

        /// <summary>
        /// Gets the type of the received contents.
        /// </summary>
        public string ContentType { get; }

        /// <summary>
        /// Gets the original underlying content stream.
        /// </summary>
        public Stream UnderlyingStream { get; }

        /// <summary>
        /// Gets the origin from where the the content stream comes from (when not set: 'unknown origin').
        /// </summary>
        /// <remarks>This could be 'unknown origin'</remarks>
        public string Origin { get; }

        /// <summary>
        /// Gets the length of the contents stream (when not set: -1)
        /// </summary>
        /// <remarks>This could be -1</remarks>
        public long Length { get; }

        /// <summary>
        /// Assign custom properties to the <see cref="ReceivedMessage" />
        /// </summary>
        /// <param name="messagingContext"></param>
        public virtual void AssignPropertiesTo(MessagingContext messagingContext) { }
    }
}