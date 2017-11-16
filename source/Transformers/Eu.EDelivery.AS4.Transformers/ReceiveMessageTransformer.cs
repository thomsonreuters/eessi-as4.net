using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Streaming;
using Eu.EDelivery.AS4.Utilities;
using NLog;

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

            if (message.UnderlyingStream.CanSeek == false)
            {
                VirtualStream messageStream = await CopyIncomingStreamToVirtualStream(message);
                message = new ReceivedMessage(messageStream, message.ContentType);
            }

            var context = new MessagingContext(message, MessagingContextMode.Receive);

            return context;
        }

        private static void PreConditions(ReceivedMessage message)
        {
            if (message.UnderlyingStream == null)
            {
                throw new InvalidMessageException("The incoming stream is not an ebMS Message");
            }

            if (!ContentTypeSupporter.IsContentTypeSupported(message.ContentType))
            {
                throw new InvalidMessageException($"ContentType is not supported {message.ContentType}{Environment.NewLine}" +
                                                  $"Supported ContentTypes are {Constants.ContentTypes.Soap} and {Constants.ContentTypes.Mime}");
            }
        }

        private static async Task<VirtualStream> CopyIncomingStreamToVirtualStream(ReceivedMessage receivedMessage)
        {
            var sw = new Stopwatch();
            sw.Start();

            VirtualStream messageStream =
                VirtualStream.CreateVirtualStream(
                    receivedMessage.UnderlyingStream.CanSeek
                        ? receivedMessage.UnderlyingStream.Length
                        : VirtualStream.ThresholdMax,
                    forAsync: true);

            if (receivedMessage.UnderlyingStream.CanSeek)
            {
                messageStream.SetLength(receivedMessage.UnderlyingStream.Length);
            }

            await receivedMessage.UnderlyingStream.CopyToFastAsync(messageStream);

            messageStream.Position = 0;
            sw.Stop();
            LogManager.GetCurrentClassLogger().Trace($"ReceiveMessageTransformer took {sw.ElapsedMilliseconds} milliseconds");
            return messageStream;
        }
    }
}
