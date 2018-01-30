using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
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
        /// <param name="cancellationToken">Cancellation which stops the transforming.</param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            var entityMessage = message as ReceivedEntityMessage;

            if (entityMessage == null)
            {
                throw new NotSupportedException(
                    "The message that must be transformed should be of type ReceivedEntityMessage");
            }

            // Get the one signal-message that must be notified.
            var as4Message = await GetAS4MessageForNotification(entityMessage, cancellationToken);

            var context = new MessagingContext(await CreateNotifyMessageEnvelope(as4Message, entityMessage.Entity.GetType()));

            await DecorateContextWithPModes(context, entityMessage);

            return context;
        }

        private static async Task<AS4Message> GetAS4MessageForNotification(ReceivedEntityMessage receivedMessage, CancellationToken cancellationToken)
        {
            if (receivedMessage.Entity is ExceptionEntity ex)
            {
                return CreateAS4ErrorFromException(ex);
            }

            if (receivedMessage is ReceivedMessageEntityMessage me)
            {
                return await RetrieveAS4MessageForNotificationFromReceivedMessage(me, cancellationToken);
            }

            throw new InvalidOperationException();
        }

        private static AS4Message CreateAS4ErrorFromException(
            ExceptionEntity exceptionEntity)
        {
            Error error = CreateSignalErrorMessage(exceptionEntity);

            AS4Message as4Message = AS4Message.Create(error, new SendingProcessingMode());

            return as4Message;
        }

        private static Error CreateSignalErrorMessage(ExceptionEntity exceptionEntity)
        {
            var errorResult = new ErrorResult(exceptionEntity.Exception, ErrorAlias.Other);

            return new ErrorBuilder()
                .WithRefToEbmsMessageId(exceptionEntity.EbmsRefToMessageId)
                .WithErrorResult(errorResult)
                .Build();
        }

        private static async Task<AS4Message> RetrieveAS4MessageForNotificationFromReceivedMessage(ReceivedMessageEntityMessage entityMessage, CancellationToken cancellationToken)
        {
            var as4Transformer = new AS4MessageTransformer();
            var messagingContext = await as4Transformer.TransformAsync(entityMessage, cancellationToken);

            var as4Message = messagingContext.AS4Message;

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
                        as4Message.PrimarySignalMessage is Receipt
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