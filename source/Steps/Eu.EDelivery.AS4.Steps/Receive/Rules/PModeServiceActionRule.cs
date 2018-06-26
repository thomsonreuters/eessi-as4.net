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
            Model.PMode.CollaborationInfo pmodeCollaboration = pmode.MessagePackaging.CollaborationInfo;
            Model.Core.CollaborationInfo messageCollaboration = userMessage.CollaborationInfo;

            if (pmodeCollaboration == null || messageCollaboration == null)
            {
                return false;
            }

            bool equalAction = 
                pmodeCollaboration.Action?.Equals(messageCollaboration.Action) == true;

            bool noServiceType = 
                pmodeCollaboration.Service?.Type == null
                && messageCollaboration.Service.Type == Maybe<string>.Nothing;

            bool equalServiceType = 
                messageCollaboration.Service.Type
                    .Select(t => pmodeCollaboration.Service?.Type == t)
                    .GetOrElse(false);

            bool equalService =
                pmodeCollaboration.Service?.Value == messageCollaboration.Service.Value
                && (noServiceType || equalServiceType);

            return equalAction && equalService;
        }
    }
}