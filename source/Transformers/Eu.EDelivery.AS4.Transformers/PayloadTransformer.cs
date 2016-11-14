using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Utilities;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    /// <summary>
    /// <see cref="ITransformer"/> implementation to transform
    /// incoming Payloads to a <see cref="InternalMessage"/>
    /// </summary>
    public class PayloadTransformer : ITransformer
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadTransformer"/>
        /// </summary>
        public PayloadTransformer()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Tranform the Payload(s)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<InternalMessage> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            var internalMessage = new InternalMessage();

            Attachment attachment = CreateAttachmentFromReceivedMessage(message);
            internalMessage.AS4Message.Attachments.Add(attachment);

            this._logger.Info("Transform the given Payload to a AS4 Attachment");
            return Task.FromResult(internalMessage);
        }

        private Attachment CreateAttachmentFromReceivedMessage(ReceivedMessage receivedMessage)
        {
            return new Attachment
            {
                Id = IdGenerator.Generate(),
                Content = receivedMessage.RequestStream,
                ContentType = receivedMessage.ContentType
            };
        }
    }
}