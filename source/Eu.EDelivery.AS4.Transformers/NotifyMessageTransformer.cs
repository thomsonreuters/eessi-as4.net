using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Factories;
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
                string ebmsMessageId = IdentifierFactory.Instance.Create();
                Error error = Error.FromErrorResult(
                    ebmsMessageId,
                    ex.EbmsRefToMessageId,
                    new ErrorResult(ex.Exception, ErrorAlias.Other));

                NotifyMessageEnvelope notifyEnvelope =
                    await CreateNotifyMessageEnvelopeAsync(AS4Message.Create(error), ebmsMessageId, ex.GetType());

                return new MessagingContext(notifyEnvelope, receivedMessage);
            }

            if (receivedMessage.Entity is MessageEntity me)
            {
                var as4Transformer = new AS4MessageTransformer();
                MessagingContext ctx = await as4Transformer.TransformAsync(receivedMessage);

                // Normally the message shouldn't have any attachments
                // but to be sure we should dispose them since we don't need attachments for notifying.
                ctx.AS4Message.CloseAttachments();

                NotifyMessageEnvelope notifyEnvelope =
                    await CreateNotifyMessageEnvelopeAsync(ctx.AS4Message, me.EbmsMessageId, me.GetType());

                ctx.ModifyContext(notifyEnvelope, receivedMessage.Entity.Id);

                return ctx;
            }

            throw new InvalidOperationException();
        }

        protected virtual async Task<NotifyMessageEnvelope> CreateNotifyMessageEnvelopeAsync(
            AS4Message as4Message, 
            string receivedEntityMessageId,
            Type receivedEntityType)
        {
            SignalMessage tobeNotifiedSignal =
                as4Message.SignalMessages.FirstOrDefault(s => s.MessageId == receivedEntityMessageId);

            NotifyMessage notifyMessage = 
                AS4MessageToNotifyMessageMapper.Convert(
                    tobeNotifiedSignal, 
                    receivedEntityType, 
                    as4Message.EnvelopeDocument ?? AS4XmlSerializer.ToSoapEnvelopeDocument(as4Message));

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