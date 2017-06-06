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
using FluentValidation.Results;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    /// <summary>
    /// Adapter to "Adapt" a SubmitMessage > AS4Message
    /// </summary>
    public class SubmitMessageXmlTransformer : ITransformer
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly SubmitMessageValidator _validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitMessageXmlTransformer" /> class.
        /// </summary>
        public SubmitMessageXmlTransformer()
        {
            _validator = new SubmitMessageValidator();
        }

        /// <summary>
        /// Transform a <see cref="SubmitMessage" />
        /// to a <see cref="MessagingContext"/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            Logger.Info("Transforming ReceivedMessage to InternalMessage");

            SubmitMessage submitMessage = TryDeserializeSubmitMessage(message.RequestStream);
            ValidateSubmitMessage(submitMessage);

            var internalMessage = new MessagingContext(submitMessage);
            //LogTransformedInformation();

            return await Task.FromResult(internalMessage);
        }

        private static SubmitMessage TryDeserializeSubmitMessage(Stream stream)
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

        private static AS4Exception ThrowDeserializeAS4Exception(Exception exception)
        {
            const string description = "Deserialize Submit Message Fails";
            Logger.Error(description);

            AS4ExceptionBuilder builder =
                AS4ExceptionBuilder.WithDescription(description, exception).WithInnerException(exception);

            return builder.Build();
        }

        private void ValidateSubmitMessage(SubmitMessage submitMessage)
        {
            _validator.Validate(submitMessage)
                      .Result(
                          happyPath: result => Logger.Debug($"Submit Message {submitMessage.MessageInfo.MessageId} is valid"),
                          unhappyPath: result =>
                          {
                              result.LogErrors(Logger);
                              throw ThrowInvalidSubmitMessageException(submitMessage);
                          });
        }

        private static AS4Exception ThrowInvalidSubmitMessageException(SubmitMessage submitMessage)
        {
            string description = $"Submit Message {submitMessage.MessageInfo.MessageId} was invalid, see logging";
            Logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithMessageIds(submitMessage.MessageInfo.MessageId)
                .Build();
        }
    }
}