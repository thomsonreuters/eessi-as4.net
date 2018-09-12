using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        /// Configures the <see cref="ITransformer"/> implementation with specific user-defined properties.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public void Configure(IDictionary<string, string> properties) { }

        /// <summary>
        /// Tranform the Payload(s)
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            Attachment attachment = CreateAttachmentFromReceivedMessage(message);
            AS4Message as4Message = AS4Message.Empty;
            as4Message.AddAttachment(attachment);

            Logger.Info("Transform the given Payload to a AS4 Attachment");
            return await Task.FromResult(new MessagingContext(as4Message, MessagingContextMode.Submit));
        }

        private static Attachment CreateAttachmentFromReceivedMessage(ReceivedMessage receivedMessage)
        {
            return new Attachment(
                receivedMessage.UnderlyingStream, 
                receivedMessage.ContentType);
        }
    }
}