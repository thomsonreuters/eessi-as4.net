using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Validators;
using FluentValidation.Results;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    /// <summary>
    /// <see cref="ITransformer"/> implementation that's responsible for transformation PMode models to Pull Messages instances.
    /// </summary>
    public class PModeToPullMessageTransformer : ITransformer
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage"/> to a Canonical <see cref="InternalMessage"/> instance.
        /// </summary>
        /// <param name="receivedMessage">Given message to transform.</param>
        /// <param name="cancellationToken">Cancellation which stops the transforming.</param>
        /// <returns></returns>
        public async Task<InternalMessage> TransformAsync(ReceivedMessage receivedMessage, CancellationToken cancellationToken)
        {
            if (receivedMessage.RequestStream == null)
            {
                return new InternalMessage(CreateAS4Exception("Invalid incoming request stream."));
            }

            return await TryCreatePullRequest(receivedMessage, cancellationToken);
        }

        private static async Task<InternalMessage> TryCreatePullRequest(ReceivedMessage receivedMessage, CancellationToken cancellationToken)
        {
            var transformedMessage = new InternalMessage();

            try
            {
                PullRequest pullRequest = await CreatePullRequest(receivedMessage, cancellationToken);
                transformedMessage.AS4Message.SignalMessages.Add(pullRequest);
                receivedMessage.AssignProperties(transformedMessage.AS4Message);
            }
            catch (AS4Exception exception)
            {
                transformedMessage.Exception = exception;
            }
            catch (Exception exception)
            {
                transformedMessage.Exception = CreateAS4Exception(exception.Message);
            }

            return transformedMessage;
        }

        private static async Task<PullRequest> CreatePullRequest(ReceivedMessage receivedMessage, CancellationToken cancellationToken)
        {
            var as4MessageTransformer = new AS4MessageTransformer();

            InternalMessage internalMessage = await as4MessageTransformer.TransformAsync(receivedMessage, cancellationToken);
            await ValidateSendingPMode(internalMessage.AS4Message.SendingPMode, cancellationToken);

            return new PullRequest(internalMessage.AS4Message.SendingPMode.MessagePackaging.Mpc);
        }

        private static async Task ValidateSendingPMode(SendingProcessingMode sendingPMode, CancellationToken cancellationToken)
        {
            var pmodeValidator = new SendingProcessingModeValidator();
            ValidationResult validationResult = await pmodeValidator.ValidateAsync(sendingPMode, cancellationToken);

            if (!validationResult.IsValid)
            {
                throw CreateAS4Exception($"Invalid Sending PMode: {sendingPMode.Id}");
            }
        }

        private static AS4Exception CreateAS4Exception(string description)
        {
            Logger.Error(description);
            return AS4ExceptionBuilder.WithDescription(description).Build();
        }
    }
}