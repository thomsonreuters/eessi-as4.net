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
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    /// <summary>
    /// <see cref="ITransformer"/> implementation to transorm 
    /// <see cref="ExceptionEntity"/> Models to <see cref="InternalMessage"/>
    /// </summary>
    public class ExceptionTransformer : ITransformer
    {
        private readonly ILogger _logger;
        private readonly ISerializerProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionTransformer"/> class
        /// </summary>
        public ExceptionTransformer()
        {
            this._logger = LogManager.GetCurrentClassLogger();
            this._provider = Registry.Instance.SerializerProvider;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionTransformer"/> class
        /// with a given <paramref name="provider"/>
        /// </summary>
        /// <param name="provider"></param>
        public ExceptionTransformer(ISerializerProvider provider)
        {
            this._provider = provider;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Transorm  <see cref="ExceptionEntity"/> Models 
        /// to <see cref="InternalMessage"/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<InternalMessage> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            ReceivedEntityMessage messageEntity = RetrieveEntityMessage(message);
            ExceptionEntity exceptionEntity = RetrieveExceptionEntity(messageEntity);

            AS4Message as4Message = CreateErrorAS4Message(exceptionEntity, cancellationToken);
            var internalMessage = new InternalMessage(as4Message);

            this._logger.Info($"[{exceptionEntity.EbmsRefToMessageId}] Exception AS4 Message is successfully transformed");
            return await Task.FromResult(internalMessage);
        }

        private AS4Message CreateErrorAS4Message(ExceptionEntity exceptionEntity, CancellationToken cancellationTokken)
        {
            Error error = CreateSignalErrorMessage(exceptionEntity);

            AS4Message as4Message = new AS4MessageBuilder()
                .WithSignalMessage(error).Build();

            as4Message.SendingPMode = GetPMode<SendingProcessingMode>(exceptionEntity.PMode);
            as4Message.ReceivingPMode = GetPMode<ReceivingProcessingMode>(exceptionEntity.PMode);
            as4Message.EnvelopeDocument = GetEnvelopeDocument(as4Message, cancellationTokken);

            return as4Message;
        }

        private XmlDocument GetEnvelopeDocument(AS4Message as4Message, CancellationToken cancellationToken)
        {
            var memoryStream = new MemoryStream();
            ISerializer serializer = this._provider.Get(Constants.ContentTypes.Soap);
            serializer.Serialize(as4Message, memoryStream, cancellationToken);

            var xmlDocument = new XmlDocument();
            memoryStream.Position = 0;
            xmlDocument.Load(memoryStream);

            return xmlDocument;
        }

        public T GetPMode<T>(string pmode) where T : class
        {
            return AS4XmlSerializer.Deserialize<T>(pmode);
        }

        private Error CreateSignalErrorMessage(ExceptionEntity exceptionEntity)
        {
            AS4Exception as4Exception = CreateAS4Exception(exceptionEntity);

            return new ErrorBuilder()
                .WithRefToEbmsMessageId(exceptionEntity.EbmsRefToMessageId)
                .WithAS4Exception(as4Exception)
                .Build();
        }

        private static AS4Exception CreateAS4Exception(ExceptionEntity exceptionEntity)
        {
            return AS4ExceptionBuilder
                .WithDescription(exceptionEntity.Exception)
                .WithMessageIds(exceptionEntity.EbmsRefToMessageId)
                .WithPModeString(exceptionEntity.PMode)
                .WithErrorCode(ErrorCode.Ebms0004)
                .Build();
        }

        private ExceptionEntity RetrieveExceptionEntity(ReceivedEntityMessage messageEntity)
        {
            var exceptionEntity = messageEntity.Entity as ExceptionEntity;
            if (exceptionEntity == null) throw ThrowNotSupportedTypeException();

            return exceptionEntity;
        }

        private ReceivedEntityMessage RetrieveEntityMessage(ReceivedMessage message)
        {
            var entityMessage = message as ReceivedEntityMessage;
            if (entityMessage == null) throw ThrowNotSupportedTypeException();

            return entityMessage;
        }

        private AS4Exception ThrowNotSupportedTypeException()
        {
            const string description = "Exception Transformer only supports Exception Entities";
            this._logger.Error(description);

            return AS4ExceptionBuilder.WithDescription(description).Build();
        }
    }
}
