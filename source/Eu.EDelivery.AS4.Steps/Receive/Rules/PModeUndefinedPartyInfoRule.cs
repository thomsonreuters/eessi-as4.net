using System;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Steps.Receive.Rules
{
    /// <summary>
    /// PMode Rule to check if the Party Info of the PMode is set
    /// </summary>
    internal class PModeUndefinedPartyInfoRule : IPModeRule
    {
        private const int Points = 7;
        private const int NotEqual = 0;

        /// <summary>
        /// Determine the points for the given Receiving PMode and UserMessage
        /// </summary>
        /// <param name="pmode"></param>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        public int DeterminePoints(ReceivingProcessingMode pmode, UserMessage userMessage)
        {
            if (pmode == null)
            {
                throw new ArgumentNullException(nameof(pmode));
            }

            if (userMessage == null)
            {
                throw new ArgumentNullException(nameof(userMessage));
            }

            PartyInfo partyInfo = pmode.MessagePackaging?.PartyInfo;

            return partyInfo == null
                   || !partyInfo.FromPartySpecified
                   && !partyInfo.ToPartySpecified
                ? Points
                : NotEqual;
        }
    }
}