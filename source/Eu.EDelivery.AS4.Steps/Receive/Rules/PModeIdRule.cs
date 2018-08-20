using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Steps.Receive.Rules
{
    /// <summary>
    /// PMode Rule to check if the PMode Id is equal to the UserMessage PMode Id
    /// </summary>
    internal class PModeIdRule : IPModeRule
    {
        private const int Points = 30;
        private const int NotEqual = 0;

        /// <summary>
        /// Determine the points for the given Receiving PMode and UserMessage
        /// </summary>
        /// <param name="pmode"></param>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        public int DeterminePoints(ReceivingProcessingMode pmode, UserMessage userMessage)
        {
            return IsPModeIdEqual(pmode, userMessage) ? Points : NotEqual;
        }

        private static bool IsPModeIdEqual(ReceivingProcessingMode pmode, UserMessage userMessage)
        {
            return userMessage.CollaborationInfo.AgreementReference?
                .Select(a => a.PModeId)
                .Flatten()
                .Equals((pmode.Id != null).ThenMaybe(pmode.Id)) ?? false;
        }
    }
}