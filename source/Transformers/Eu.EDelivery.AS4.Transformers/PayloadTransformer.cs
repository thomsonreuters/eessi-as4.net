using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    /// <summary>
    /// <see cref="ITransformer"/> implementation to transform
    /// incoming Payloads to a <see cref="MessagingContext"/>
    /// </summary>
    public class PayloadTransformer : ITransformer
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Tranform the Payload(s)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            Attachment attachment = CreateAttachmentFromReceivedMessage(message);
            AS4Message as4Message = AS4Message.Empty;
            as4Message.AddAttachment(attachment);

            Logger.Info("Transform the given Payload to a AS4 Attachment");
            return await Task.FromResult(new MessagingContext(as4Message, MessagingContextMode.Submit));
        }

        private static Attachment CreateAttachmentFromReceivedMessage(ReceivedMessage receivedMessage)
        {
            return new Attachment
            {
                Content = receivedMessage.UnderlyingStream,
                ContentType = receivedMessage.ContentType
            };
        }
    }
}