using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    public class ExceptionToNotifyMessageTransformer : ITransformer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ISerializerProvider _provider;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionToNotifyMessageTransformer"/> class.
        /// </summary>
        public ExceptionToNotifyMessageTransformer()
        {
            _provider = Registry.Instance.SerializerProvider;
        }

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage"/> to a Canonical <see cref="MessagingContext"/> instance.
        /// </summary>
        /// <param name="message">Given message to transform.</param>
        /// <param name="cancellationToken">Cancellation which stops the transforming.</param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            ReceivedEntityMessage messageEntity = RetrieveEntityMessage(message);
            ExceptionEntity exceptionEntity = RetrieveExceptionEntity(messageEntity);

            AS4Message as4Message = await CreateErrorAS4Message(exceptionEntity, cancellationToken);

            var internalMessage = new MessagingContext(await CreateNotifyMessageEnvelope(as4Message))
            {
                SendingPMode = await GetPMode<SendingProcessingMode>(exceptionEntity.PMode),
                ReceivingPMode = await GetPMode<ReceivingProcessingMode>(exceptionEntity.PMode),
            };

            Logger.Info($"[{exceptionEntity.EbmsRefToMessageId}] Exception AS4 Message is successfully transformed");

            return internalMessage;
        }

        private static ReceivedEntityMessage RetrieveEntityMessage(ReceivedMessage message)
        {
            var entityMessage = message as ReceivedEntityMessage;
            if (entityMessage == null)
            {
                throw ThrowNotSupportedTypeException();
            }

            return entityMessage;
        }

        private static ExceptionEntity RetrieveExceptionEntity(ReceivedEntityMessage messageEntity)
        {
            var exceptionEntity = messageEntity.Entity as ExceptionEntity;
            if (exceptionEntity == null)
            {
                throw ThrowNotSupportedTypeException();
            }

            return exceptionEntity;
        }

        private static NotSupportedException ThrowNotSupportedTypeException()
        {
            const string description = "Exception Transformer only supports Exception Entities";
            Logger.Error(description);

            return new NotSupportedException(description);
        }

        private async Task<AS4Message> CreateErrorAS4Message(ExceptionEntity exceptionEntity, CancellationToken cancellationTokken)
        {
            Error error = CreateSignalErrorMessage(exceptionEntity);

            AS4Message as4Message = AS4Message.Create(error, new SendingProcessingMode());
            as4Message.EnvelopeDocument = await GetEnvelopeDocument(as4Message, cancellationTokken);

            return as4Message;
        }

        private static Error CreateSignalErrorMessage(ExceptionEntity exceptionEntity)
        {
            return new ErrorBuilder()
                .WithRefToEbmsMessageId(exceptionEntity.EbmsRefToMessageId)
                .WithErrorResult(new ErrorResult(exceptionEntity.Exception, ErrorCode.Ebms0004, ErrorAlias.Other))
                .Build();
        }

        private static Task<T> GetPMode<T>(string pmode) where T : class
        {
            return AS4XmlSerializer.FromStringAsync<T>(pmode);
        }

        private async Task<XmlDocument> GetEnvelopeDocument(AS4Message as4Message, CancellationToken cancellationToken)
        {
            using (var memoryStream = new MemoryStream())
            {
                ISerializer serializer = _provider.Get(Constants.ContentTypes.Soap);
                await serializer.SerializeAsync(as4Message, memoryStream, cancellationToken);

                var xmlDocument = new XmlDocument() { PreserveWhitespace = true };
                memoryStream.Position = 0;
                xmlDocument.Load(memoryStream);

                return xmlDocument;
            }
        }

        protected virtual async Task<NotifyMessageEnvelope> CreateNotifyMessageEnvelope(AS4Message as4Message)
        {
            var notifyMessage = AS4MessageToNotifyMessageMapper.Convert(as4Message);

            if (notifyMessage?.StatusInfo != null)
            {
                notifyMessage.StatusInfo.Status = Status.Exception;
            }

            var serialized = await AS4XmlSerializer.ToStringAsync(notifyMessage);

            return new NotifyMessageEnvelope(notifyMessage.MessageInfo,
                                             notifyMessage.StatusInfo.Status,
                                             System.Text.Encoding.UTF8.GetBytes(serialized),
                                             "application/xml");
        }
    }
}