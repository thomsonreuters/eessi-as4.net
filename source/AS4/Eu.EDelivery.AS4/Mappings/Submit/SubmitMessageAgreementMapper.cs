using Castle.Core.Internal;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// Mapping the <see cref="AgreementReference"/>
    /// </summary>
    public class SubmitMessageAgreementMapper : ISubmitMapper
    {
        private const string NotAllowedByTheSendingPMode = "Submit Message is not allowed by the Sending PMode ";

        /// <summary>
        /// 1.SubmitMessage / CollaborationInfo / Agreement
        /// </summary>
        /// <param name="submitMessage"></param>
        /// <param name="userMessage"></param>
        public void Map(SubmitMessage submitMessage, UserMessage userMessage)
        {
            Agreement submitMessageRef = submitMessage.Collaboration.AgreementRef;
            AgreementReference pmodeRef = submitMessage.PMode.MessagePackaging.CollaborationInfo?.AgreementReference;
            AgreementReference userMessageRef = userMessage.CollaborationInfo.AgreementReference;

            // 1. IF (PMode / Message Packaging / IncludePModeId = true) > PMode / Id
            // 2. ELSE > No pmode attribute
            userMessageRef.PModeId = submitMessage.PMode.MessagePackaging.IncludePModeId ? submitMessage.PMode.Id : null;
            AllowOverridePrecondition(submitMessage);

            userMessageRef.Name = submitMessageRef.Value ?? pmodeRef?.Name;
            userMessageRef.Type = submitMessageRef.RefType ?? pmodeRef?.Type;
        }

        private void AllowOverridePrecondition(SubmitMessage submitMessage)
        {
            Agreement submitMessageRef = submitMessage.Collaboration.AgreementRef;
            AgreementReference pmodeRef = submitMessage.PMode.MessagePackaging.CollaborationInfo?.AgreementReference;

            if (DoesSubmitMessageTriesToOverridePModeValues(submitMessage, submitMessageRef.Value, pmodeRef?.Name))
                throw new AS4Exception(NotAllowedByTheSendingPMode + submitMessage.PMode.Id + " to override Agreement Ref Value");

            if (DoesSubmitMessageTriesToOverridePModeValues(submitMessage, submitMessageRef.RefType, pmodeRef?.Type))
                throw new AS4Exception(NotAllowedByTheSendingPMode + submitMessage.PMode.Id + " to override Agreement Ref Type");
        }

        private bool DoesSubmitMessageTriesToOverridePModeValues(SubmitMessage submitMessage, params string[] values)
        {
            return submitMessage.PMode.AllowOverride == false && IsNotNullOrEmpty(values);
        }

        private bool IsNotNullOrEmpty(params string[] values)
        {
            var isNotNullOrEmpty = true;
            values.ForEach(v => isNotNullOrEmpty = !string.IsNullOrEmpty(v));

            return isNotNullOrEmpty;
        }
    }
}
