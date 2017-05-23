using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Validators;
using FluentValidation.Results;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// Add the retrieved PMode to the <see cref="SubmitMessage" />
    /// after the PMode is verified
    /// </summary>
    public class RetrieveSendingPModeStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IConfig _config;
        private readonly SendingProcessingModeValidator _validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetrieveSendingPModeStep" /> class
        /// </summary>
        public RetrieveSendingPModeStep() : this(Config.Instance) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="RetrieveSendingPModeStep" /> class
        /// Create a new Retrieve PMode Step with a given <see cref="IConfig" />
        /// </summary>
        /// <param name="config">
        /// </param>
        public RetrieveSendingPModeStep(IConfig config)
        {
            _config = config;
            _validator = new SendingProcessingModeValidator();
        }

        /// <summary>
        /// Start retrieving the PMode for the <see cref="SubmitMessage" />
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="AS4Exception">Thrown when PMode doesn't get retrieved</exception>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage message, CancellationToken cancellationToken)
        {
            try
            {
                RetrieveSendPMode(message);
                return await StepResult.SuccessAsync(message);
            }
            catch (Exception exception)
            {
                throw ThrowAS4CannotRetrieveSendPModeException(message, exception);
            }
        }

        private void RetrieveSendPMode(InternalMessage message)
        {
            SendingProcessingMode pmode = RetrievePMode(message);

            ValidatePMode(pmode);

            message.SubmitMessage.PMode = pmode;
        }

        private SendingProcessingMode RetrievePMode(InternalMessage message)
        {
            string processingModeId = RetrieveProcessingModeId(message.SubmitMessage.Collaboration);

            SendingProcessingMode pmode = _config.GetSendingPMode(processingModeId);

            Logger.Info($"{message.Prefix} Sending PMode {pmode.Id} was retrieved");

            return pmode;
        }

        private string RetrieveProcessingModeId(CollaborationInfo collaborationInfo)
        {
            if (collaborationInfo == null)
            {
                throw new ArgumentNullException(nameof(collaborationInfo));
            }

            return collaborationInfo.AgreementRef?.PModeId;
        }

        private void ValidatePMode(SendingProcessingMode pmode)
        {
            ValidationResult result = _validator.Validate(pmode);

            if (result.IsValid)
            {
                Logger.Info($"Sending PMode {pmode.Id} is valid for Submit Message");
            }
            else
            {
                throw ThrowHandleInvalidPModeException(pmode, result);
            }
        }

        private static AS4Exception ThrowHandleInvalidPModeException(IPMode pmode, ValidationResult result)
        {
            foreach (ValidationFailure e in result.Errors)
            {
                Logger.Error($"Sending PMode Validation Error: {e.PropertyName} = {e.ErrorMessage}");
            }

            string description = $"Sending PMode {pmode.Id} was invalid, see logging";
            Logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithMessageIds(Guid.NewGuid().ToString())
                .Build();
        }

        private static AS4Exception ThrowAS4CannotRetrieveSendPModeException(
            InternalMessage internalMessage,
            Exception exception)
        {
            string generatedMessageId = Guid.NewGuid().ToString();

            return AS4ExceptionBuilder
                .WithDescription($"[generated: {generatedMessageId}] Cannot retrieve Sending PMode", exception)
                .WithInnerException(exception)
                .WithSendingPMode(internalMessage.AS4Message.SendingPMode)
                .WithMessageIds(generatedMessageId)
                .Build();
        }
    }
}