using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
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
        private readonly ISerializerProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AS4MessageTransformer" /> class.
        /// </summary>
        public AS4MessageTransformer() : this(Registry.Instance.SerializerProvider) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AS4MessageTransformer" /> class.
        /// with a given <paramref name="provider" />
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <exception cref="ArgumentNullException">provider</exception>
        public AS4MessageTransformer(ISerializerProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            _provider = provider;
        }

        /// <summary>
        /// Transform to a <see cref="MessagingContext" />
        /// with a <see cref="AS4Message" /> included
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            Logger.Debug("Transform AS4 Message to Messaging Context");
            PreConditions(message);

            return await TransformMessage(message, cancellationToken);
        }

        private void PreConditions(ReceivedMessage message)
        {
            if (message.UnderlyingStream == null)
            {
                throw new InvalidDataException("The incoming stream is not an ebMS Message");
            }

            if (!ContentTypeSupporter.IsContentTypeSupported(message.ContentType))
            {
                throw new InvalidDataException($"ContentType is not supported {nameof(message.ContentType)}");
            }
        }

        private async Task<MessagingContext> TransformMessage(
            ReceivedMessage receivedMessage,
            CancellationToken cancellation)
        {
            VirtualStream messageStream = await CopyIncomingStreamToVirtualStream(receivedMessage);

            AS4Message as4Message = await DeserializeMessage(receivedMessage.ContentType, messageStream, cancellation);

            var context = new MessagingContext(as4Message, MessagingContextMode.Unknown);
            receivedMessage.AssignPropertiesTo(context);

            return context;
        }

        private static async Task<VirtualStream> CopyIncomingStreamToVirtualStream(ReceivedMessage receivedMessage)
        {
            if (receivedMessage.UnderlyingStream is VirtualStream stream)
            {
                return stream;
            }

            VirtualStream messageStream =
                VirtualStream.CreateVirtualStream(
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
            ISerializer serializer = _provider.Get(contentType);
            return await serializer.DeserializeAsync(virtualStream, contentType, cancellation);
        }
    }
}