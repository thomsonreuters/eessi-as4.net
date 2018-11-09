using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Transformers
{
    public class NotifyMessageTransformer : ITransformer
    {
        /// <summary>
        /// Configures the <see cref="ITransformer"/> implementation with specific user-defined properties.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public void Configure(IDictionary<string, string> properties) { }

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage"/> to a Canonical <see cref="MessagingContext"/> instance.
        /// </summary>
        /// <param name="message">Given message to transform.</param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (!(message is ReceivedEntityMessage receivedMessage))
            {
                throw new NotSupportedException(
                    $"Incoming message stream from {message.Origin} that must be transformed should be of type {nameof(ReceivedEntityMessage)}");
            }

            if (receivedMessage.Entity is ExceptionEntity ex)
            {
                Error error = Error.FromErrorResult(
                    ex.EbmsRefToMessageId,
                    new ErrorResult(ex.Exception, ErrorAlias.Other));

                NotifyMessageEnvelope notifyEnvelope =
                    await CreateNotifyMessageEnvelope(
                        AS4Message.Create(error, new SendingProcessingMode()),
                        receivedMessage.GetType());

                return new MessagingContext(notifyEnvelope, receivedMessage.Entity.Id);
            }

            if (receivedMessage.Entity is MessageEntity me)
            {
                MessagingContext ctx =
                    await RetrieveAS4MessageForNotificationFromReceivedMessage(me.EbmsMessageId, receivedMessage);

                NotifyMessageEnvelope notifyEnvelope =
                    await CreateNotifyMessageEnvelope(ctx.AS4Message, receivedMessage.Entity.GetType());

                ctx.ModifyContext(notifyEnvelope, receivedMessage.Entity.Id);

                return ctx;
            }

            throw new InvalidOperationException();
        }

        private static async Task<MessagingContext> RetrieveAS4MessageForNotificationFromReceivedMessage(string ebmsMessageId, ReceivedMessage entityMessage)
        {
            var as4Transformer = new AS4MessageTransformer();
            MessagingContext deserializedAS4MessageContext = await as4Transformer.TransformAsync(entityMessage);

            AS4Message as4Message = deserializedAS4MessageContext.AS4Message;

            // No attachments are needed in order to create notify messages.
            as4Message.RemoveAllAttachments();

            // Remove all signal-messages except the one that we should be notifying
            // Create the DeliverMessage for this specific UserMessage that has been received.
            SignalMessage signalMessage = 
                as4Message.SignalMessages
                          .FirstOrDefault(m => StringComparer.OrdinalIgnoreCase.Equals(m.MessageId, ebmsMessageId));

            if (signalMessage == null)
            {
                throw new InvalidOperationException(
                    $"Incoming SignalMessage from {entityMessage.Origin} with ID " + 
                    $"{ebmsMessageId} could not be found in the referenced AS4Message");
            }

            return deserializedAS4MessageContext;
        }

        protected virtual async Task<NotifyMessageEnvelope> CreateNotifyMessageEnvelope(AS4Message as4Message, Type receivedEntityType)
        {
            // TODO: the DeliverMessage is created in the steps, but the NotifyMessage in the Transformer?
            NotifyMessage notifyMessage = AS4MessageToNotifyMessageMapper.Convert(as4Message, receivedEntityType);

            var serialized = await AS4XmlSerializer.ToStringAsync(notifyMessage).ConfigureAwait(false);

            return new NotifyMessageEnvelope(
                notifyMessage.MessageInfo,
                notifyMessage.StatusInfo.Status,
                System.Text.Encoding.UTF8.GetBytes(serialized),
                "application/xml",
                receivedEntityType);
        }
    }
}