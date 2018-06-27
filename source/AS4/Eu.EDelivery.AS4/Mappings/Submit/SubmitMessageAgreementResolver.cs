using System;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using CoreAgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;
using PModeAgreementReference = Eu.EDelivery.AS4.Model.PMode.AgreementReference;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// Mapping the <see cref="CoreAgreementReference"/>
    /// </summary>
    public static class SubmitMessageAgreementResolver
    {
        private const string NotAllowedByTheSendingPMode = "Submit Message is not allowed by the Sending PMode ";

        /// <summary>
        /// 1.SubmitMessage / CollaborationInfo / Agreement
        /// </summary>
        /// <param name="submit"></param>
        /// <param name="user"></param>
        public static Maybe<CoreAgreementReference> ResolveAgreementReference(SubmitMessage submit, UserMessage user)
        {
            SendingProcessingMode sendPMode = submit.PMode;
            Agreement submitAgreement = submit.Collaboration.AgreementRef;
            PModeAgreementReference pmodeAgreement = sendPMode.MessagePackaging.CollaborationInfo?.AgreementReference;

            EnsureSubmitMessageDoesntOverridePMode(submitAgreement, sendPMode);

            string value = submitAgreement.Value ?? pmodeAgreement?.Value;
            string type = submitAgreement.RefType ?? pmodeAgreement?.Type;

            return (value != null)
                .ThenMaybe(value)
                .Select(v => new CoreAgreementReference(
                    value: v,
                    type: (type != null).ThenMaybe(type),
                    pmodeId: (sendPMode.Id != null).ThenMaybe(sendPMode.Id).Where(_ => sendPMode.MessagePackaging.IncludePModeId)));
        }

        private static void EnsureSubmitMessageDoesntOverridePMode(Agreement submitAgreement, SendingProcessingMode sendPMode)
        {
            PModeAgreementReference pmodeAgreement = sendPMode.MessagePackaging.CollaborationInfo?.AgreementReference;

            if (DoesSubmitMessageTriesToOverridePModeValues(sendPMode, submitAgreement.Value, pmodeAgreement?.Value))
            {
                throw new InvalidOperationException(
                    $"{NotAllowedByTheSendingPMode}{sendPMode.Id} to override Agreement Ref Value");
            }

            if (DoesSubmitMessageTriesToOverridePModeValues(sendPMode, submitAgreement.RefType, pmodeAgreement?.Type))
            {
                throw new InvalidOperationException(
                    $"{NotAllowedByTheSendingPMode}{sendPMode.Id} to override Agreement Ref Type");
            }
        }

        private static bool DoesSubmitMessageTriesToOverridePModeValues(
            SendingProcessingMode pmode, 
            string submitValue, 
            string pmodeValue)
        {
            return pmode.AllowOverride == false &&
                   !string.IsNullOrEmpty(pmodeValue) &&
                   !string.IsNullOrEmpty(submitValue);
        }
    }
}