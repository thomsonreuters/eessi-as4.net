using System.Configuration;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Validators;

namespace AS4.ParserService.CustomSteps
{
    public class ValidateSendingPModeStep : IStep
    {
        /// <summary>
        /// Validates whether the configured Sending PMode is valid and can be used.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            var result = SendingProcessingModeValidator.Instance.Validate(messagingContext.SendingPMode);

            if (result.IsValid)
            {
                return StepResult.SuccessAsync(messagingContext);
            }

            string description = result.AppendValidationErrorsToErrorMessage($"Sending PMode {messagingContext.SendingPMode.Id} was invalid:");

            return StepResult.FailedAsync(new MessagingContext(new ConfigurationErrorsException(description)));
        }
    }
}