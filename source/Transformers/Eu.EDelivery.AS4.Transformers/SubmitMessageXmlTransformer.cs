using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
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
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

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

            SubmitMessage submitMessage = DeserializeSubmitMessage(message.UnderlyingStream);
            ValidateSubmitMessage(submitMessage);

            return await Task.FromResult(new MessagingContext(submitMessage));
        }

        private static SubmitMessage DeserializeSubmitMessage(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(SubmitMessage));
            return serializer.Deserialize(stream) as SubmitMessage;
        }

        private static void ValidateSubmitMessage(SubmitMessage submitMessage)
        {
            var validator = new SubmitMessageValidator();

            validator.Validate(submitMessage)
                     .Result(
                         result => Logger.Debug($"Submit Message {submitMessage.MessageInfo.MessageId} is valid"),
                         result =>
                         {
                             result.LogErrors(Logger);
                             throw ThrowInvalidSubmitMessageException(submitMessage);
                         });
        }

        private static InvalidMessageException ThrowInvalidSubmitMessageException(SubmitMessage submitMessage)
        {
            string description = $"Submit Message {submitMessage.MessageInfo.MessageId} was invalid, see logging";
            Logger.Error(description);

            return new InvalidMessageException(description);
        }
    }
}