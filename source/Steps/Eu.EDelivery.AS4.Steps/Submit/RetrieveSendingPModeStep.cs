using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Validators;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// Add the retrieved PMode to the <see cref="SubmitMessage" />
    /// after the PMode is verified
    /// </summary>
    public class RetrieveSendingPModeStep : IStep
    {
        private readonly IConfig _config;
        private readonly ILogger _logger;
        private readonly IValidator<SendingProcessingMode> _validator;
        private InternalMessage _internalMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetrieveSendingPModeStep"/> class
        /// </summary>
        public RetrieveSendingPModeStep()
        {
            this._config = Config.Instance;
            this._validator = new SendingProcessingModeValidator();
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetrieveSendingPModeStep"/> class
        /// Create a new Retrieve PMode Step with a given <see cref="IConfig"/>
        /// </summary>
        /// <param name="config">
        /// </param>
        public RetrieveSendingPModeStep(IConfig config)
        {
            this._config = config;
            this._validator = new SendingProcessingModeValidator();
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start retrieving the PMode for the <see cref="SubmitMessage" />
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="AS4Exception">Thrown when PMode doesn't get retrieved</exception>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(InternalMessage message, CancellationToken cancellationToken)
        {
            try
            {
                this._internalMessage = message;
                RetrieveSendPMode();
                return StepResult.SuccessAsync(message);
            }
            catch (Exception exception)
            {
                throw ThrowAS4CannotRetrieveSendPModeException(message, exception);
            }
        }

        private void RetrieveSendPMode()
        {
            SendingProcessingMode pmode = RetrievePMode();
            ValidatePMode(pmode);
            this._internalMessage.SubmitMessage.PMode = pmode;
        }

        private SendingProcessingMode RetrievePMode()
        {
            string processingModeId = this._internalMessage.SubmitMessage.Collaboration?.AgreementRef?.PModeId;
            SendingProcessingMode pmode = this._config.GetSendingPMode(processingModeId);
            this._logger.Debug($"{this._internalMessage.Prefix} Sending PMode {pmode.Id} was retrieved");

            return pmode;
        }

        private void ValidatePMode(SendingProcessingMode pmode)
        {
            if (this._validator.Validate(pmode))
                this._logger.Info($"Sending PMode {pmode.Id} is valid for Submit Message");
        }

        private AS4Exception ThrowAS4CannotRetrieveSendPModeException(InternalMessage internalMessage, Exception exception)
        {
            string generatedMessageId = Guid.NewGuid().ToString();

            return new AS4ExceptionBuilder()
                .WithInnerException(exception)
                .WithDescription($"[generated: {generatedMessageId}] Cannot retrieve Sending PMode")
                .WithSendingPMode(internalMessage.AS4Message.SendingPMode)
                .WithMessageIds(generatedMessageId)
                .Build();
        }
    }
}