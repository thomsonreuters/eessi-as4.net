using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Streaming;
using Eu.EDelivery.AS4.Utilities;

namespace Eu.EDelivery.AS4.Transformers
{
    public class ReceiveMessageTransformer : ITransformer
    {
        /// <summary>
        /// Transform a given <see cref="ReceivedMessage"/> to a Canonical <see cref="MessagingContext"/> instance.
        /// </summary>
        /// <param name="message">Given message to transform.</param>
        /// <param name="cancellationToken">Cancellation which stops the transforming.</param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            PreConditions(message);

            VirtualStream messageStream = await CopyIncomingStreamToVirtualStream(message);

            var context = new MessagingContext(new ReceivedMessage(messageStream, message.ContentType), MessagingContextMode.Receive);

            return context;
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

        private static async Task<VirtualStream> CopyIncomingStreamToVirtualStream(ReceivedMessage receivedMessage)
        {
            VirtualStream messageStream =
                VirtualStream.CreateVirtualStream(
                    receivedMessage.UnderlyingStream.CanSeek
                        ? receivedMessage.UnderlyingStream.Length
                        : VirtualStream.ThresholdMax);

            await receivedMessage.UnderlyingStream.CopyToAsync(messageStream);

            messageStream.Position = 0;

            return messageStream;
        }
    }
}
