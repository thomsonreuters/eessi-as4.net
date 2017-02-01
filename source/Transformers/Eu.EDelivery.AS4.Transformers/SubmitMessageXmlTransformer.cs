using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Validators;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    /// <summary>
    /// Adapter to "Adapt" a SubmitMessage > AS4Message
    /// </summary>
    public class SubmitMessageXmlTransformer : ITransformer
    {
        private readonly IValidator<SubmitMessage> _validator;
        private readonly ILogger _logger;

        /// <summary>
        /// Create a <see cref="SubmitMessage"/> Transformer
        /// to transform from Xml
        /// </summary>
        public SubmitMessageXmlTransformer()
        {
            this._validator = new SubmitMessageValidator();
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Transform a <see cref="SubmitMessage" />
        /// to a <see cref="InternalMessage"/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<InternalMessage> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            _logger.Info($"Transforming ReceivedMessage {message.Id} to InternalMessage");
            
            SubmitMessage submitMessage = TryDeserializeSubmitMessage(message.RequestStream);
            ValidateSubmitMessage(submitMessage);

            var internalMessage = new InternalMessage(submitMessage);
            LogTransformedInformation();

            return Task.FromResult(internalMessage);
        }

        private SubmitMessage TryDeserializeSubmitMessage(Stream stream)
        {
            try
            {
                return DeserializeSubmitMessage(stream);
            }
            catch (Exception exception)
            {
                throw ThrowDeserializeAS4Exception(exception);
            }
        }

        private static SubmitMessage DeserializeSubmitMessage(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(SubmitMessage));
            return serializer.Deserialize(stream) as SubmitMessage;
        }

        private AS4Exception ThrowDeserializeAS4Exception(Exception exception)
        {
            const string description = "Deserialize Submit Message Fails";
            this._logger.Error(description);

            return new AS4ExceptionBuilder()
                .WithInnerException(exception)
                .WithDescription(description)
                .Build();
        }

        private void ValidateSubmitMessage(SubmitMessage submitMessage)
        {
            if (this._validator.Validate(submitMessage))
                this._logger.Debug($"Submit Message {submitMessage.MessageInfo.MessageId} is valid");
        }

        private void LogTransformedInformation()
        {
            this._logger.Info("SubmitMessage is successfully tranfromed from Xml");
        }
    }
}