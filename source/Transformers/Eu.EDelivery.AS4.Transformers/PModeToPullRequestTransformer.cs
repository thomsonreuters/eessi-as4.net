using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
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
    /// <see cref="ITransformer"/> implementation that's responsible for transformation PMode models to Pull Messages instances.
    /// </summary>
    public class PModeToPullRequestTransformer : ITransformer
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage"/> to a Canonical <see cref="InternalMessage"/> instance.
        /// </summary>
        /// <param name="receivedMessage">Given message to transform.</param>
        /// <param name="cancellationToken">Cancellation which stops the transforming.</param>
        /// <returns></returns>
        public Task<InternalMessage> TransformAsync(ReceivedMessage receivedMessage, CancellationToken cancellationToken)
        {
            if (receivedMessage.RequestStream == null)
            {
                return Task.FromResult(new InternalMessage(CreateAS4Exception("Invalid incoming request stream.")));
            }

            return TryCreatePullRequest(receivedMessage);
        }

        private static Task<InternalMessage> TryCreatePullRequest(ReceivedMessage receivedMessage)
        {
            var transformedMessage = new InternalMessage();

            try
            {
                receivedMessage.AssignPropertiesTo(transformedMessage.AS4Message);

                SendingProcessingMode pmode = DeserializeValidPMode(receivedMessage);
                transformedMessage.AS4Message.SendingPMode = pmode;
                transformedMessage.AS4Message.SignalMessages.Add(new PullRequest(pmode.PullConfiguration.Mpc));
            }
            catch (AS4Exception exception)
            {
                transformedMessage.Exception = exception;
            }
            catch (Exception exception)
            {
                transformedMessage.Exception = CreateAS4Exception(exception.Message);
            }

            return Task.FromResult(transformedMessage);
        }

        private static SendingProcessingMode DeserializeValidPMode(ReceivedMessage receivedMessage)
        {
            var pmode = AS4XmlSerializer.FromStream<SendingProcessingMode>(receivedMessage.RequestStream);
            var validator = new SendingProcessingModeValidator();

            ValidationResult result = validator.Validate(pmode);

            if (result.IsValid)
            {
                return pmode;
            }

            throw ThrowHandleInvalidPModeException(pmode, result);
        }

        private static AS4Exception ThrowHandleInvalidPModeException(IPMode pmode, ValidationResult result)
        {
            foreach (ValidationFailure error in result.Errors)
            {
                Logger.Error($"Sending PMode Validation Error: {error.PropertyName} = {error.ErrorMessage}");
            }

            string description = $"Sending PMode {pmode.Id} was invalid, see logging";
            Logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithMessageIds(Guid.NewGuid().ToString())
                .Build();
        }

        private static AS4Exception CreateAS4Exception(string description)
        {
            Logger.Error(description);
            return AS4ExceptionBuilder.WithDescription(description).Build();
        }
    }
}