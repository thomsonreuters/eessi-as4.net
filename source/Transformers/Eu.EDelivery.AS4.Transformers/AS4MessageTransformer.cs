using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Utilities;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    /// <summary>
    /// Transform <see cref="ReceivedMessage" />
    /// to a <see cref="InternalMessage" /> with an <see cref="AS4Message" />
    /// </summary>
    public class AS4MessageTransformer : ITransformer
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ISerializerProvider _provider;

        /// <summary>
        /// Initializes a new intance of the <see cref="AS4MessageTransformer"/> class
        /// </summary>
        public AS4MessageTransformer()
        {
            _provider = Registry.Instance.SerializerProvider;
        }

        /// <summary>
        /// Iniitializes a new instance of the <see cref="AS4MessageTransformer"/> class
        /// with a given <paramref name="provider"/>
        /// </summary>
        /// <param name="provider"></param>
        public AS4MessageTransformer(ISerializerProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }
            _provider = provider;            
        }

        /// <summary>
        /// Transform to a <see cref="InternalMessage"/>
        /// with a <see cref="AS4Message"/> included
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<InternalMessage> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            Logger.Debug("Transform AS4 Message to Internal Message");
            AS4Message as4Message = await TryTransformMessage(message, cancellationToken);

            return new InternalMessage(as4Message);
        }

        private async Task<AS4Message> TryTransformMessage(ReceivedMessage message, CancellationToken cancellationToken)
        {
            try
            {
                PreConditions(message);
                return await TransformMessage(message, cancellationToken);
            }
            catch (AS4Exception exception)
            {
                Error error = CreateError(exception);
                return CreateErrorMessage(error);
            }
        }

        private static Error CreateError(AS4Exception exception)
        {
            return new ErrorBuilder()
                .WithAS4Exception(exception)
                .BuildWithOriginalAS4Exception();
        }

        private static AS4Message CreateErrorMessage(Error errorMessage)
        {
            return new AS4MessageBuilder()
                .WithSignalMessage(errorMessage)
                .Build();
        }

        private async Task<AS4Message> TransformMessage(ReceivedMessage receivedMessage,
            CancellationToken cancellationToken)
        {
            ISerializer serializer = _provider.Get(receivedMessage.ContentType);
            AS4Message as4Message = await serializer
                .DeserializeAsync(receivedMessage.RequestStream, receivedMessage.ContentType, cancellationToken);

            receivedMessage.AssignPropertiesTo(as4Message);

            return as4Message;
        }

        private void PreConditions(ReceivedMessage message)
        {
            if (message.RequestStream == null)
            {
                throw ThrowAS4TransformException("The incoming stream is not an ebMS Message");
            }

            if (!ContentTypeSupporter.IsContentTypeSupported(message.ContentType))
            {
                throw ThrowAS4TransformException($"ContentType is not supported {nameof(message.ContentType)}");
            }
        }

        private static AS4Exception ThrowAS4TransformException(string description)
        {
            Logger.Error(description);

            throw AS4ExceptionBuilder
                .WithDescription(description)
                .WithErrorCode(ErrorCode.Ebms0009)
                .Build();
        }
    }
}