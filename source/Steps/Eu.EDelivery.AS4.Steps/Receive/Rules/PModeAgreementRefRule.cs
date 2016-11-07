using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Steps.Receive.Rules
{
    /// <summary>
    /// PMode Rule to check if the PMode Agreement Ref is equal to the UserMessage Agreement Ref
    /// </summary>
    public class PModeAgreementRefRule : IPModeRule
    {
        private const int Points = 4;
        private const int NotEqual = 0;

        /// <summary>
        /// Determine the points for the given Receiving PMode and UserMessage
        /// </summary>
        /// <param name="pmode"></param>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        public int DeterminePoints(ReceivingProcessingMode pmode, UserMessage userMessage)
        {
            return IsAgreementRefEqual(pmode, userMessage) ? Points : NotEqual;
        }

        private bool IsAgreementRefEqual(ReceivingProcessingMode pmode, UserMessage userMessage)
        {
            AgreementReference pmodeAgreementRef = pmode.MessagePackaging.CollaborationInfo?.AgreementReference;
            AgreementReference userMessageAgreementRef = userMessage.CollaborationInfo?.AgreementReference;

            return AgreementRefIsPresent(pmodeAgreementRef, userMessageAgreementRef) &&
                   pmodeAgreementRef.Equals(userMessageAgreementRef);
        }

        private bool AgreementRefIsPresent(
            AgreementReference pmodeAgreementRef,
            AgreementReference userMessageAgreementRef)
        {
            return pmodeAgreementRef != null && userMessageAgreementRef != null;
        }
    }
}