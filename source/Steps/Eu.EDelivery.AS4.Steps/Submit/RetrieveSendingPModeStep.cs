using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Common;
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
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetrieveSendingPModeStep" /> class
        /// </summary>
        public RetrieveSendingPModeStep() : this(Config.Instance) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetrieveSendingPModeStep" /> class
        /// Create a new Retrieve PMode Step with a given <see cref="IConfig" />
        /// </summary>
        /// <param name="config">
        /// </param>
        public RetrieveSendingPModeStep(IConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Start retrieving the PMode for the <see cref="SubmitMessage" />
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            messagingContext.SubmitMessage.PMode = RetrieveSendPMode(messagingContext);
            messagingContext.SendingPMode = messagingContext.SubmitMessage.PMode;

            return await StepResult.SuccessAsync(messagingContext);
        }

        private SendingProcessingMode RetrieveSendPMode(MessagingContext message)
        {
            SendingProcessingMode pmode = RetrievePMode(message);
            ValidatePMode(pmode);

            return pmode;
        }

        private SendingProcessingMode RetrievePMode(MessagingContext message)
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

        private static void ValidatePMode(SendingProcessingMode pmode)
        {            
            SendingProcessingModeValidator.Instance.Validate(pmode).Result(
                onValidationSuccess: result => Logger.Info($"Sending PMode {pmode.Id} is valid for Submit Message"),
                onValidationFailed: result =>
                {
                    string description = result.AppendValidationErrorsToErrorMessage($"Sending PMode {pmode.Id} was invalid:");
                    
                    Logger.Error(description);

                    throw new ConfigurationErrorsException(description);
                });
        }
    }
}