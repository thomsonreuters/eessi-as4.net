using System;
using System.Linq;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Steps.Receive.Rules
{
    /// <summary>
    /// PMode Rule to check if the PMode Parties are equal to the UserMessage Parties
    /// </summary>
    internal class PModePartyInfoRule : IPModeRule
    {
        private const int ToPartyPoints = 8;
        private const int FromPartyPoints = 7;
        private const int PartyRolePoints = 1;
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

            PartyInfo pmodePartyInfo = pmode.MessagePackaging?.PartyInfo;
            if (pmodePartyInfo == null)
            {
                return NotEqual;
            }
            
            if (!pmodePartyInfo.FromPartySpecified 
                && !pmodePartyInfo.ToPartySpecified)
            {
                return NotEqual;
            }

            var points = 0;

            bool fromPartyEqual = ArePartyIdsEqual(pmodePartyInfo.FromParty, userMessage.Sender);
            bool toPartyEqual = ArePartyIdsEqual(pmodePartyInfo.ToParty, userMessage.Receiver);

            if (fromPartyEqual && !pmodePartyInfo.ToPartySpecified)
            {
                points += FromPartyPoints;
            }
            
            if (toPartyEqual && !pmodePartyInfo.FromPartySpecified)
            {
                points += ToPartyPoints;
            }

            if (fromPartyEqual && toPartyEqual)
            {
                points += FromPartyPoints + ToPartyPoints;
            }

            if (ArePartyRolesEqual(pmodePartyInfo, userMessage))
            {
                points += PartyRolePoints;
            }

            return points;
        }

        private static bool ArePartyIdsEqual(
            Model.PMode.Party pmodeParty, 
            Model.Core.Party messageParty)
        {
            if (pmodeParty == null)
            {
                return false;
            }

            return pmodeParty.PartyIds.Any(x => messageParty.PartyIds.Any(y =>
            {
                bool noType = 
                    x?.Type == null 
                    && y.Type == Maybe<string>.Nothing;

                bool equalTypes = 
                    y.Type
                     .Select(t => StringComparer.OrdinalIgnoreCase.Equals(t, x?.Type))
                     .GetOrElse(false);

                bool equalIds =
                    StringComparer
                        .OrdinalIgnoreCase
                        .Equals(x?.Id, y.Id);

                return equalIds && (equalTypes || noType);
            }));
        }

        private static bool ArePartyRolesEqual(PartyInfo pmodePartyInfo, UserMessage userMessage)
        {
            if (pmodePartyInfo?.FromParty == null 
                || pmodePartyInfo?.ToParty == null)
            {
                return false;
            }

            bool equalFromRoles = 
                StringComparer
                    .OrdinalIgnoreCase
                    .Equals(pmodePartyInfo.FromParty.Role, userMessage.Sender.Role);

            bool equalToRoles = 
                StringComparer
                    .OrdinalIgnoreCase
                    .Equals(pmodePartyInfo.ToParty.Role, userMessage.Receiver.Role);

            return equalFromRoles && equalToRoles;
        }

    }
}