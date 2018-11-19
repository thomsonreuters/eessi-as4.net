using System;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using AgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;
using PModeAgreementReference = Eu.EDelivery.AS4.Model.PMode.AgreementReference;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// Mapping the <see cref="AgreementReference"/>
    /// </summary>
    internal static class SubmitMessageAgreementResolver
    {
        /// <summary>
        /// 1.SubmitMessage / CollaborationInfo / Agreement
        /// </summary>
        /// <param name="submit"></param>
        public static Maybe<AgreementReference> ResolveAgreementReference(SubmitMessage submit)
        {
            if (submit == null)
            {
                throw new ArgumentNullException(nameof(submit));
            }

            if (submit.PMode == null)
            {
                throw new ArgumentNullException(nameof(submit.PMode));
            }

            SendingProcessingMode sendPMode = submit.PMode;
            PModeAgreementReference pmodeAgreement = sendPMode.MessagePackaging?.CollaborationInfo?.AgreementReference;
            Agreement submitAgreement = submit.Collaboration?.AgreementRef;

            if (sendPMode.AllowOverride == false 
                && !String.IsNullOrEmpty(submitAgreement?.Value)
                && !String.IsNullOrEmpty(pmodeAgreement?.Value)
                && !StringComparer.OrdinalIgnoreCase.Equals(pmodeAgreement?.Value, submitAgreement?.Value))
            {
                throw new InvalidOperationException(
                    $"SubmitMessage is not allowed by the Sending PMode {sendPMode.Id} to override AgreementReference.Value");
            }

            if (!String.IsNullOrEmpty(submitAgreement?.Value))
            {
                if (submitAgreement?.RefType != null)
                {
                    if (sendPMode.MessagePackaging?.IncludePModeId == true)
                    {
                        return Maybe.Just(
                            new AgreementReference(
                                submitAgreement?.Value,
                                submitAgreement?.RefType,
                                sendPMode.Id));
                    }
                    
                    return Maybe.Just(
                        new AgreementReference(
                            submitAgreement?.Value, 
                            Maybe.Just(submitAgreement?.RefType),
                            Maybe<string>.Nothing));
                }

                if (sendPMode.MessagePackaging?.IncludePModeId == true)
                {
                    return Maybe.Just(
                        new AgreementReference(
                            submitAgreement?.Value,
                            Maybe<string>.Nothing,
                            Maybe.Just(sendPMode.Id)));
                }

                return Maybe.Just(
                    new AgreementReference(submitAgreement?.Value));
            }

            if (!String.IsNullOrEmpty(pmodeAgreement?.Value))
            {
                if (pmodeAgreement?.Type != null)
                {
                    if (sendPMode.MessagePackaging?.IncludePModeId == true)
                    {
                        return Maybe.Just(
                            new AgreementReference(
                                pmodeAgreement?.Value,
                                pmodeAgreement?.Type,
                                sendPMode.Id));
                    }

                    return Maybe.Just(
                        new AgreementReference(
                            pmodeAgreement?.Value,
                            Maybe.Just(pmodeAgreement?.Type),
                            Maybe<string>.Nothing));
                }

                if (sendPMode.MessagePackaging?.IncludePModeId == true)
                {
                    return Maybe.Just(
                        new AgreementReference(
                            pmodeAgreement?.Value,
                            Maybe<string>.Nothing,
                            Maybe.Just(sendPMode.Id)));
                }

                return Maybe.Just(
                    new AgreementReference(pmodeAgreement?.Value));
            }
            
            return Maybe<AgreementReference>.Nothing;
        }
    }
}