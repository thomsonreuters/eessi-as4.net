using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Steps.Receive.Rules
{
    /// <summary>
    /// PMode Rule to check if the PMode Parties are equal to the UserMessage Parties
    /// </summary>
    internal class PModePartyInfoRule : IPModeRule
    {
        private const int Points = 15;
        private const int NotEqual = 0;

        /// <summary>
        /// Determine the points for the given Receiving PMode and UserMessage
        /// </summary>
        /// <param name="pmode"></param>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        public int DeterminePoints(ReceivingProcessingMode pmode, UserMessage userMessage)
        {
            return IsPartyInfoEqual(pmode, userMessage) ? Points : NotEqual;
        }

        private bool IsPartyInfoEqual(ReceivingProcessingMode pmode, UserMessage userMessage)
        {
            if (pmode.MessagePackaging.PartyInfo == null) return false;
            PartyInfo partyInfo = pmode.MessagePackaging.PartyInfo;
            if (partyInfo.IsEmpty()) return false;

            return 
                partyInfo.FromParty.Equals(userMessage.Sender) && 
                partyInfo.ToParty.Equals(userMessage.Receiver);
        }
    }
}