using System;
using System.ComponentModel;
using System.Configuration;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Validators;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// Add the retrieved PMode to the <see cref="Model.Submit.SubmitMessage" />
    /// after the PMode is verified
    /// </summary>
    [Info("Retrieve SendingPMode")]
    [Description("Retrieve the SendingPMode that must be used to send the AS4Message")]
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
        /// Retrieve the PMode that must be used to send the SubmitMessage that is in the current Messagingcontext />
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
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

        private SendingProcessingMode RetrievePMode(MessagingContext context)
        {
            string processingModeId = RetrieveProcessingModeId(context.SubmitMessage.Collaboration);

            SendingProcessingMode pmode = _config.GetSendingPMode(processingModeId);

            Logger.Info($"{context.LogTag} SendingPMode {pmode.Id} was successfully retrieved");

            return pmode;
        }

        private string RetrieveProcessingModeId(Model.Common.CollaborationInfo collaborationInfo)
        {
            if (collaborationInfo == null)
            {
                Logger.Error(
                    "(Submit) Unable to retrieve SendingPMode: " +
                    "no <PModeId/> element was found in the SubmitMessage. " +
                    "This element is needed to locate the right SendingPMode, " + 
                    "please provide one inside the SubmitMessage.CollaborationInfo.AgreementRef element");

                throw new ArgumentNullException(nameof(collaborationInfo));
            }

            return collaborationInfo.AgreementRef?.PModeId;
        }

        private static void ValidatePMode(SendingProcessingMode pmode)
        {
            SendingProcessingModeValidator.Instance.Validate(pmode).Result(
                onValidationSuccess: result => Logger.Debug($"SendingPMode {pmode.Id} is valid for Submit Message"),
                onValidationFailed: result =>
                {
                    string description = 
                        result.AppendValidationErrorsToErrorMessage(
                            $"(Submit) SendingPMode {pmode.Id} was invalid and cannot be used to assign to the SubmitMessage:");

                    Logger.Error(description);

                    throw new ConfigurationErrorsException(description);
                });
        }
    }
}