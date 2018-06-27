using Eu.EDelivery.AS4.Model.PMode;
using AgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;

namespace Eu.EDelivery.AS4.Mappings.PMode
{
    /// <summary>
    /// Resolve the <see cref="AgreementReference"/> from the <see cref="SendingProcessingMode"/>
    /// </summary>
    public static class PModeAgreementRefResolver
    {
        /// <summary>
        /// 2. PMode / Message Packaging / CollaborationInfo / AgreementRef / Value 
        /// </summary>
        /// <param name="pmode"></param>
        /// <returns></returns>
        public static Maybe<AgreementReference> ResolveAgreementReference(SendingProcessingMode pmode)
        {
            var pmodeAgreement =
                pmode.MessagePackaging
                     .CollaborationInfo
                     ?.AgreementReference;

            string value = pmodeAgreement?.Value;
            if (value == null)
            {
                return Maybe<AgreementReference>.Nothing;
            }

            Maybe<string> type =
                (pmodeAgreement?.Type != null)
                .ThenMaybe(pmodeAgreement?.Type);

            Maybe<string> pmodeId =
                (pmodeAgreement?.PModeId != null)
                .ThenMaybe(pmodeAgreement?.PModeId)
                .Where(_ => pmode.MessagePackaging.IncludePModeId);

            return Maybe.Just(new AgreementReference(value, type, pmodeId));
        }
    }
}
