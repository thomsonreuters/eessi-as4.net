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
            var entityMessage = message as ReceivedEntityMessage;
            if (entityMessage == null)
            {
                throw new NotSupportedException(
                    $"Incoming message stream from {message.Origin} that must be transformed should be of type {nameof(ReceivedEntityMessage)}");
            }

            // Get the one signal-message that must be notified.
            var as4Message = await GetAS4MessageForNotification(entityMessage);

            var context = new MessagingContext(
                await CreateNotifyMessageEnvelope(as4Message, entityMessage.Entity.GetType()),
                entityMessage.Entity.Id);

            await DecorateContextWithPModes(context, entityMessage);

            return context;
        }

        private static async Task<AS4Message> GetAS4MessageForNotification(ReceivedEntityMessage receivedMessage)
        {
            if (receivedMessage.Entity is ExceptionEntity ex)
            {
                Error error = Error.FromErrorResult(
                    ex.EbmsRefToMessageId, 
                    new ErrorResult(ex.Exception, ErrorAlias.Other));

                return AS4Message.Create(error, new SendingProcessingMode());
            }

            if (receivedMessage.Entity is MessageEntity me)
            {
                return await RetrieveAS4MessageForNotificationFromReceivedMessage(me.EbmsMessageId, receivedMessage);
            }

            throw new InvalidOperationException();
        }

        private static async Task<AS4Message> RetrieveAS4MessageForNotificationFromReceivedMessage(string ebmsMessageId, ReceivedMessage entityMessage)
        {
            var as4Transformer = new AS4MessageTransformer();
            var messagingContext = await as4Transformer.TransformAsync(entityMessage);

            var as4Message = messagingContext.AS4Message;

            // No attachments are needed in order to create notify messages.
            as4Message.RemoveAllAttachments();

            // Remove all signal-messages except the one that we should be notifying
            // Create the DeliverMessage for this specific UserMessage that has been received.
            var signalMessage = 
                as4Message.SignalMessages.FirstOrDefault(m => m.MessageId.Equals(ebmsMessageId, StringComparison.OrdinalIgnoreCase));

            if (signalMessage == null)
            {
                throw new InvalidOperationException(
                    $"Incoming SignalMessage from {entityMessage.Origin} with ID " + 
                    $"{ebmsMessageId} could not be found in the referenced AS4Message");
            }

            return as4Message;
        }

        protected virtual async Task<NotifyMessageEnvelope> CreateNotifyMessageEnvelope(AS4Message as4Message, Type receivedEntityType)
        {
            var notifyMessage = AS4MessageToNotifyMessageMapper.Convert(as4Message);

            if (notifyMessage?.StatusInfo != null)
            {
                if (typeof(ExceptionEntity).IsAssignableFrom(receivedEntityType))
                {
                    notifyMessage.StatusInfo.Status = Status.Exception;
                }
                else
                {
                    notifyMessage.StatusInfo.Status =
                        as4Message.FirstSignalMessage is Receipt
                            ? Status.Delivered
                            : Status.Error;
                }
            }

            var serialized = await AS4XmlSerializer.ToStringAsync(notifyMessage).ConfigureAwait(false);

            return new NotifyMessageEnvelope(notifyMessage.MessageInfo,
                                             notifyMessage.StatusInfo.Status,
                                             System.Text.Encoding.UTF8.GetBytes(serialized),
                                             "application/xml",
                                             receivedEntityType);
        }

        private static async Task DecorateContextWithPModes(MessagingContext context, ReceivedEntityMessage message)
        {
            string pmode = string.Empty;

            if (message.Entity is MessageEntity me)
            {
                pmode = me.PMode;
            }
            else if (message.Entity is ExceptionEntity ee)
            {
                pmode = ee.PMode;
            }

            context.ReceivingPMode = await AS4XmlSerializer.FromStringAsync<ReceivingProcessingMode>(pmode);
            context.SendingPMode = await AS4XmlSerializer.FromStringAsync<SendingProcessingMode>(pmode);
        }

    }
}