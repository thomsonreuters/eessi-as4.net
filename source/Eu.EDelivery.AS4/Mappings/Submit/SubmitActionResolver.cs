using System;
using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// 1. SubmitMessage / CollaborationInfo / Action
    /// 2. PMode / Message Packaging / CollaborationInfo / Action
    /// 3. Default Test Action Namespace
    /// </summary>
    public static class SubmitActionResolver
    {
        /// <summary>
        /// Resolve the Action
        /// </summary>
        /// <param name="submitMessage"></param>
        /// <returns></returns>
        public static string ResolveAction(SubmitMessage submitMessage)
        {
            if (submitMessage.PMode.AllowOverride == false && DoesSubmitMessageTriesToOverridePModeValues(submitMessage))
            {
                throw new InvalidOperationException(
                    $"Submit Message is not allowed by PMode {submitMessage.PMode.Id} to override Action");
            }

            if (!string.IsNullOrEmpty(submitMessage.Collaboration.Action))
            {
                return submitMessage.Collaboration.Action;
            }

            SendingProcessingMode pmode = submitMessage.PMode;
            return PModeActionResolver.ResolveAction(pmode);
        }

        private static bool DoesSubmitMessageTriesToOverridePModeValues(SubmitMessage submitMessage)
        {
            return !string.IsNullOrEmpty(submitMessage.Collaboration.Action)
                   && !string.IsNullOrEmpty(submitMessage.PMode.MessagePackaging?.CollaborationInfo?.Action)
                   && !StringComparer.OrdinalIgnoreCase.Equals(
                       submitMessage.Collaboration.Action,
                       submitMessage.PMode.MessagePackaging?.CollaborationInfo?.Action);
        }
    }
}