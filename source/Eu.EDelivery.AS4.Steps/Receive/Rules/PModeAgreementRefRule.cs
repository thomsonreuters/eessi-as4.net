using System;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Steps.Receive.Rules
{
    /// <summary>
    /// PMode Rule to check if the PMode Agreement Ref is equal to the UserMessage Agreement Ref
    /// </summary>
    internal class PModeAgreementRefRule : IPModeRule
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
            if (pmode == null)
            {
                throw new ArgumentNullException(nameof(pmode));
            }

            if (userMessage == null)
            {
                throw new ArgumentNullException(nameof(userMessage));
            }

            Model.PMode.AgreementReference pmodeAgreement = 
                pmode.MessagePackaging
                     ?.CollaborationInfo
                     ?.AgreementReference;

            Model.Core.AgreementReference userAgreement = 
                userMessage.CollaborationInfo
                           .AgreementReference
                           .GetOrElse(() => null);

            if (pmodeAgreement == null || userAgreement == null)
            {
                return NotEqual;
            }

            bool equalPModeId =
                userAgreement.PModeId
                   .Select(id => StringComparer.OrdinalIgnoreCase.Equals(id, pmodeAgreement?.PModeId))
                   .GetOrElse(false);

            bool noPModeId =
                userAgreement.PModeId == Maybe<string>.Nothing
                && pmodeAgreement?.PModeId == null;

            bool equalType =
                userAgreement.Type
                   .Select(t => StringComparer.OrdinalIgnoreCase.Equals(t, pmodeAgreement?.Type))
                   .GetOrElse(false);

            bool noType =
                userAgreement.Type == Maybe<string>.Nothing
                && pmodeAgreement?.Type == null;

            bool equalValue =
                StringComparer
                    .OrdinalIgnoreCase
                    .Equals(userAgreement.Value, pmodeAgreement?.Value);

            return (equalPModeId || noPModeId)
                   && (equalType || noType)
                   && equalValue
                ? Points
                : NotEqual;
        }
    }
}