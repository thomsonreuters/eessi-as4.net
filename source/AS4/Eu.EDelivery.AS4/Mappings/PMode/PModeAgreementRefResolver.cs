using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Mappings.PMode
{
    /// <summary>
    /// Resolve the <see cref="AgreementReference"/> from the <see cref="SendingProcessingMode"/>
    /// </summary>
    public class PModeAgreementRefResolver : IPModeResolver<AgreementReference>
    {
        /// <summary>
        /// 2. PMode / Message Packaging / CollaborationInfo / AgreementRef / Value 
        /// </summary>
        /// <param name="pmode"></param>
        /// <returns></returns>
        public AgreementReference Resolve(SendingProcessingMode pmode)
        {
            var agreementRef = new AgreementReference();
            if (pmode.MessagePackaging.IncludePModeId)
                agreementRef = new AgreementReference(pmode.Id);

            AgreementReference pmodeRef = pmode.MessagePackaging
                .CollaborationInfo?.AgreementReference;

            agreementRef.Value = pmodeRef?.Value;
            agreementRef.Type = pmodeRef?.Type;

            return agreementRef;
        }
    }
}
