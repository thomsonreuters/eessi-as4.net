using System.IO;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Validators;
using FluentValidation.Results;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    /// <summary>
    /// <see cref="ITransformer" /> implementation that's responsible for transformation PMode models to Pull Messages
    /// instances.
    /// </summary>
    public class PModeToPullRequestTransformer : ITransformer
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage" /> to a Canonical <see cref="MessagingContext" /> instance.
        /// </summary>
        /// <param name="receivedMessage">Given message to transform.</param>
        /// <param name="cancellationToken">Cancellation which stops the transforming.</param>
        /// <returns></returns>
        public Task<MessagingContext> TransformAsync(
            ReceivedMessage receivedMessage,
            CancellationToken cancellationToken)
        {
            if (receivedMessage.RequestStream == null)
            {
                throw new InvalidDataException("Invalid incoming request stream.");
            }

            return CreatePullRequest(receivedMessage);
        }

        private static async Task<MessagingContext> CreatePullRequest(ReceivedMessage receivedMessage)
        {
            SendingProcessingMode pmode = await DeserializeValidPMode(receivedMessage);

            AS4Message pullRequestMessage = AS4Message.Create(new PullRequest(pmode.PullConfiguration.Mpc), pmode);

            MemoryStream ms = new MemoryStream();
            var serializer = SerializerProvider.Default.Get(pullRequestMessage.ContentType);
            serializer.Serialize(pullRequestMessage, ms, CancellationToken.None);
            ms.Position = 0;

            var context = new MessagingContext(new ReceivedMessage(ms, pullRequestMessage.ContentType), MessagingContextMode.Receive);
            context.SendingPMode = pmode;

            return context;
        }

        private static async Task<SendingProcessingMode> DeserializeValidPMode(ReceivedMessage receivedMessage)
        {
            SendingProcessingMode pmode =
                await AS4XmlSerializer.FromStreamAsync<SendingProcessingMode>(receivedMessage.RequestStream);
            var validator = new SendingProcessingModeValidator();

            ValidationResult result = validator.Validate(pmode);

            if (result.IsValid)
            {
                return pmode;
            }

            throw ThrowHandleInvalidPModeException(pmode, result);
        }

        private static InvalidDataException ThrowHandleInvalidPModeException(IPMode pmode, ValidationResult result)
        {
            foreach (ValidationFailure error in result.Errors)
            {
                Logger.Error($"Sending PMode Validation Error: {error.PropertyName} = {error.ErrorMessage}");
            }

            string description = $"Sending PMode {pmode.Id} was invalid, see logging";
            Logger.Error(description);

            return new InvalidDataException(description);
        }
    }
}