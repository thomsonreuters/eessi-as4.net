using Castle.Core.Internal;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// Mapping the <see cref="AgreementReference"/>
    /// </summary>
    public class SubmitMessageAgreementMapper : ISubmitMapper
    {
        private const string NotAllowedByTheSendingPMode = "Submit Message is not allowed by the Sending PMode ";
        private SendingProcessingMode _pmode;

        /// <summary>
        /// 1.SubmitMessage / CollaborationInfo / Agreement
        /// </summary>
        /// <param name="submitMessage"></param>
        /// <param name="userMessage"></param>
        public void Map(SubmitMessage submitMessage, UserMessage userMessage)
        {
            this._pmode = submitMessage.PMode;

            Agreement submitMessageRef = submitMessage.Collaboration.AgreementRef;
            AgreementReference pmodeRef = this._pmode.MessagePackaging.CollaborationInfo?.AgreementReference;
            AgreementReference userMessageRef = userMessage.CollaborationInfo.AgreementReference;

            // 1. IF (PMode / Message Packaging / IncludePModeId = true) > PMode / Id
            // 2. ELSE > No pmode attribute
            userMessageRef.PModeId = this._pmode.MessagePackaging.IncludePModeId ? this._pmode.Id : null;
            AllowOverridePrecondition(submitMessage);

            userMessageRef.Value = submitMessageRef.Value ?? pmodeRef?.Value;
            userMessageRef.Type = submitMessageRef.RefType ?? pmodeRef?.Type;
        }

        private void AllowOverridePrecondition(SubmitMessage submitMessage)
        {
            Agreement submitMessageRef = submitMessage.Collaboration.AgreementRef;
            AgreementReference pmodeRef = this._pmode.MessagePackaging.CollaborationInfo?.AgreementReference;

            if (DoesSubmitMessageTriesToOverridePModeValues(submitMessageRef.Value, pmodeRef?.Value))
                throw new AS4Exception(NotAllowedByTheSendingPMode + submitMessage.PMode.Id +
                                       " to override Agreement Ref Value");

            if (DoesSubmitMessageTriesToOverridePModeValues(submitMessageRef.RefType, pmodeRef?.Type))
                throw new AS4Exception(NotAllowedByTheSendingPMode + submitMessage.PMode.Id +
                                       " to override Agreement Ref Type");
        }

        private bool DoesSubmitMessageTriesToOverridePModeValues(string submitValue, string pmodeValue)
        {
            return this._pmode.AllowOverride == false &&
                   !string.IsNullOrEmpty(pmodeValue) &&
                   !string.IsNullOrEmpty(submitValue);
        }
    }
}