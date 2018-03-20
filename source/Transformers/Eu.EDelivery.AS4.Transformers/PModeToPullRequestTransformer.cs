using System.Collections.Generic;
using System.IO;
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
        /// Configures the <see cref="ITransformer"/> implementation with specific user-defined properties.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public void Configure(IDictionary<string, string> properties) { }

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage" /> to a Canonical <see cref="MessagingContext" /> instance.
        /// </summary>
        /// <param name="receivedMessage">Given message to transform.</param>
        /// <returns></returns>
        public Task<MessagingContext> TransformAsync(ReceivedMessage receivedMessage)
        {
            if (receivedMessage.UnderlyingStream == null)
            {
                throw new InvalidDataException("Invalid incoming request stream.");
            }

            return CreatePullRequest(receivedMessage);
        }

        private static async Task<MessagingContext> CreatePullRequest(ReceivedMessage receivedMessage)
        {
            SendingProcessingMode pmode = await DeserializeValidPMode(receivedMessage);

            AS4Message pullRequestMessage = AS4Message.Create(new PullRequest(pmode.MessagePackaging.Mpc), pmode);
          
            var context = new MessagingContext(pullRequestMessage, MessagingContextMode.Receive);

            context.SendingPMode = pmode;

            return context;
        }

        private static async Task<SendingProcessingMode> DeserializeValidPMode(ReceivedMessage receivedMessage)
        {
            SendingProcessingMode pmode =
                await AS4XmlSerializer.FromStreamAsync<SendingProcessingMode>(receivedMessage.UnderlyingStream);
            
            ValidationResult result = SendingProcessingModeValidator.Instance.Validate(pmode);

            if (result.IsValid)
            {
                return pmode;
            }

            throw CreateInvalidPModeException(pmode, result);
        }

        private static InvalidDataException CreateInvalidPModeException(IPMode pmode, ValidationResult result)
        {
            var errorMessage = result.AppendValidationErrorsToErrorMessage($"Receiving PMode {pmode.Id} is not valid");

            Logger.Error(errorMessage);
            
            return new InvalidDataException(errorMessage);
        }
    }
}