using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Utilities;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    /// <summary>
    /// Transform <see cref="ReceivedMessage" />
    /// to a <see cref="MessagingContext" /> with an <see cref="AS4Message" />
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
        /// Transform to a <see cref="MessagingContext"/>
        /// with a <see cref="AS4Message"/> included
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {           
            try
            {
                Logger.Debug("Transform AS4 Message to Internal Message");
                PreConditions(message);

                return await TransformMessage(message, cancellationToken);
            }
            catch (AS4Exception exception)
            {
                Error error = CreateError(exception);
                return new MessagingContext(CreateErrorMessage(error), MessagingContextMode.Unknown);
            }
        }

        private static Error CreateError(AS4Exception exception)
        {
            return new ErrorBuilder()
                .WithAS4Exception(exception)
                .BuildWithOriginalAS4Exception();
        }

        private static AS4Message CreateErrorMessage(SignalMessage errorMessage)
        {
            return AS4Message.Create(errorMessage);
        }

        private async Task<MessagingContext> TransformMessage(ReceivedMessage receivedMessage,
            CancellationToken cancellationToken)
        {
            ISerializer serializer = _provider.Get(receivedMessage.ContentType);
            AS4Message as4Message = await serializer
                .DeserializeAsync(receivedMessage.RequestStream, receivedMessage.ContentType, cancellationToken);

            var message = new MessagingContext(as4Message, MessagingContextMode.Unknown);
            receivedMessage.AssignPropertiesTo(message);

            return message;
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