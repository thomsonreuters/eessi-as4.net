using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Transformers
{
    public class SignalMessageToNotifyMessageTransformer : ITransformer
    {
        /// <summary>
        /// Transform a given <see cref="ReceivedMessage"/> to a Canonical <see cref="MessagingContext"/> instance.
        /// </summary>
        /// <param name="message">Given message to transform.</param>
        /// <param name="cancellationToken">Cancellation which stops the transforming.</param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            var entityMessage = message as ReceivedMessageEntityMessage;

            if (entityMessage == null)
            {
                throw new NotSupportedException(
                    "The message that must be transformed should be of type ReceivedMessageEntityMessage");
            }

            // Get the AS4Message that is referred to by this entityMessage and modify it so that it just contains
            // the one usermessage that should be delivered.
            AS4Message as4Message = await RetrieveAS4SignalMessage(entityMessage, cancellationToken);

            return new MessagingContext(await CreateNotifyMessageEnvelope(as4Message));
        }

        protected virtual async Task<NotifyMessageEnvelope> CreateNotifyMessageEnvelope(AS4Message as4Message)
        {
            var notifyMessage = AS4MessageToNotifyMessageMapper.Convert(as4Message);

            if (notifyMessage?.StatusInfo != null)
            {
                notifyMessage.StatusInfo.Status = 
                    as4Message.PrimarySignalMessage is Receipt
                        ? Status.Delivered
                        : Status.Error;
            }

            var serialized = await AS4XmlSerializer.ToStringAsync(notifyMessage);

            return new NotifyMessageEnvelope(notifyMessage.MessageInfo,
                                             notifyMessage.StatusInfo.Status,
                                             System.Text.Encoding.UTF8.GetBytes(serialized),                  
                                             "application/xml");
        }
        
        private static async Task<AS4Message> RetrieveAS4SignalMessage(ReceivedMessageEntityMessage entityMessage, CancellationToken cancellationToken)
        {
            var as4Transformer = new AS4MessageTransformer();
            var internalMessage = await as4Transformer.TransformAsync(entityMessage, cancellationToken);

            var as4Message = internalMessage.AS4Message;

            // No attachments are needed in order to create notify messages.
            as4Message.CloseAttachments();
            as4Message.Attachments.Clear();

            // Remove all signal-messages except the one that we should be notifying
            // Create the DeliverMessage for this specific UserMessage that has been received.
            var signalMessage =
                as4Message.SignalMessages.FirstOrDefault(m => m.MessageId.Equals(entityMessage.MessageEntity.EbmsMessageId, StringComparison.OrdinalIgnoreCase));

            if (signalMessage == null)
            {
                throw new InvalidOperationException($"The SignalMessage with ID {entityMessage.MessageEntity.EbmsMessageId} could not be found in the referenced AS4Message.");
            }

            return as4Message;
        }
    }
}
