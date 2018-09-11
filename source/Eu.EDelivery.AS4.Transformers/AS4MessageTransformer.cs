using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Streaming;
using Eu.EDelivery.AS4.Utilities;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    /// <summary>
    /// Transform <see cref="ReceivedMessage" />
    /// to a <see cref="MessagingContext" /> with an <see cref="AS4Message" />
    /// </summary>
    public class AS4MessageTransformer : ITransformer
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Configures the <see cref="ITransformer"/> implementation with specific user-defined properties.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public void Configure(IDictionary<string, string> properties) { }

        /// <summary>
        /// Transform to a <see cref="MessagingContext" />
        /// with a <see cref="AS4Message" /> included
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message)
        {
            if (message?.UnderlyingStream == null)
            {
                throw new InvalidDataException(
                    $"The incoming stream from {message.Origin} is not an ebMS Message");
            }

            if (!ContentTypeSupporter.IsContentTypeSupported(message.ContentType))
            {
                throw new InvalidDataException(
                    $"ContentType {nameof(message.ContentType)} is not supported");
            }

            VirtualStream messageStream = await CopyIncomingStreamToVirtualStream(message);
            AS4Message as4Message = await DeserializeMessage(message.ContentType, messageStream, CancellationToken.None);

            var context = new MessagingContext(message, MessagingContextMode.Unknown);
            context.ModifyContext(as4Message);
            message.AssignPropertiesTo(context);

            return context;
        }

        private static async Task<VirtualStream> CopyIncomingStreamToVirtualStream(ReceivedMessage receivedMessage)
        {
            if (receivedMessage.UnderlyingStream is VirtualStream stream)
            {
                return stream;
            }

            VirtualStream messageStream =
                VirtualStream.Create(
                    receivedMessage.UnderlyingStream.CanSeek
                        ? receivedMessage.UnderlyingStream.Length
                        : VirtualStream.ThresholdMax,
                    forAsync: true);
            
            await receivedMessage.UnderlyingStream.CopyToFastAsync(messageStream);

            messageStream.Position = 0;

            return messageStream;
        }

        private async Task<AS4Message> DeserializeMessage(
            string contentType,
            Stream virtualStream,
            CancellationToken cancellation)
        {
            ISerializer serializer = SerializerProvider.Default.Get(contentType);
            return await serializer.DeserializeAsync(virtualStream, contentType, cancellation);
        }
    }
}