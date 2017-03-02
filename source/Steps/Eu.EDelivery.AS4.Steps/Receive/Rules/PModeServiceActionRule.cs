using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Steps.Receive.Rules
{
    /// <summary>
    /// PMode Rule to check if the PMode Service/Action is equal to the UserMessage Service/Action
    /// </summary>
    internal class PModeServiceActionRule : IPModeRule
    {
        private const int Points = 3;
        private const int NotEqual = 0;

        /// <summary>
        /// Determine the points for the given Receiving PMode and UserMessage
        /// </summary>
        /// <param name="pmode"></param>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        public int DeterminePoints(ReceivingProcessingMode pmode, UserMessage userMessage)
        {
            return ServiceActionCondition(pmode, userMessage) ? Points : NotEqual;
        }

        private static bool ServiceActionCondition(ReceivingProcessingMode pmode, UserMessage userMessage)
        {
            if (pmode.MessagePackaging.CollaborationInfo == null)
            {
                return false;
            }

            CollaborationInfo pmodeCollaboration = pmode.MessagePackaging.CollaborationInfo;
            CollaborationInfo messageCollaboration = userMessage.CollaborationInfo;

            return pmodeCollaboration.Action?.Equals(messageCollaboration.Action) == true &&
                   pmodeCollaboration.Service?.Equals(messageCollaboration.Service) == true;
        }
    }
}