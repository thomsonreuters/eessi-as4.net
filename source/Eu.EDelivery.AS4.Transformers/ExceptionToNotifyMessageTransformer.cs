using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    [Obsolete("Use the NotifyMessageTransformer instead")]
    public class ExceptionToNotifyMessageTransformer : ITransformer
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Configures the <see cref="ITransformer"/> implementation with specific user-defined properties.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public void Configure(IDictionary<string, string> properties) { }

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage" /> to a Canonical <see cref="MessagingContext" /> instance.
        /// </summary>
        /// <param name="message">Given message to transform.</param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message)
        {
            ReceivedEntityMessage messageEntity = RetrieveEntityMessage(message);
            ExceptionEntity exceptionEntity = RetrieveExceptionEntity(messageEntity);

            AS4Message as4Message = CreateErrorAS4Message(exceptionEntity);

            var messagingContext = new MessagingContext(await CreateNotifyMessageEnvelope(as4Message, exceptionEntity.GetType()))
            {
                SendingPMode = await GetPMode<SendingProcessingMode>(exceptionEntity.PMode),
                ReceivingPMode = await GetPMode<ReceivingProcessingMode>(exceptionEntity.PMode)
            };

            Logger.Info($"[{exceptionEntity.EbmsRefToMessageId}] Exception AS4 Message is successfully transformed");

            return messagingContext;
        }

        private static ReceivedEntityMessage RetrieveEntityMessage(ReceivedMessage message)
        {
            if (message is ReceivedEntityMessage entityMessage)
            {
                return entityMessage;
            }

            throw new NotSupportedException($"Exception Transformer only supports '{nameof(ReceivedEntityMessage)}'");
        }

        private static ExceptionEntity RetrieveExceptionEntity(ReceivedEntityMessage messageEntity)
        {
            if (messageEntity.Entity is ExceptionEntity exceptionEntity)
            {
                return exceptionEntity;
            }

            throw new NotSupportedException($"Exception Notify Transformer only supports '{nameof(ExceptionEntity)}'");
        }

        private static AS4Message CreateErrorAS4Message(ExceptionEntity exceptionEntity)
        {
            Error error = CreateSignalErrorMessage(exceptionEntity);

            return AS4Message.Create(error, new SendingProcessingMode());
        }

        private static Error CreateSignalErrorMessage(ExceptionEntity exceptionEntity)
        {
            return Error.FromErrorResult(
                exceptionEntity.EbmsRefToMessageId ?? IdentifierFactory.Instance.Create(), 
                new ErrorResult(exceptionEntity.Exception, ErrorAlias.Other));
        }

        private static Task<T> GetPMode<T>(string pmode) where T : class
        {
            return AS4XmlSerializer.FromStringAsync<T>(pmode);
        }

        protected virtual async Task<NotifyMessageEnvelope> CreateNotifyMessageEnvelope(AS4Message as4Message, Type receivedEntityType)
        {
            NotifyMessage notifyMessage = AS4MessageToNotifyMessageMapper.Convert(as4Message);

            if (notifyMessage?.StatusInfo != null)
            {
                notifyMessage.StatusInfo.Status = Status.Exception;
            }

            string serialized = await AS4XmlSerializer.ToStringAsync(notifyMessage);

            return new NotifyMessageEnvelope(
                notifyMessage.MessageInfo,
                notifyMessage.StatusInfo.Status,
                Encoding.UTF8.GetBytes(serialized),
                "application/xml",
                receivedEntityType);
        }
    }
}