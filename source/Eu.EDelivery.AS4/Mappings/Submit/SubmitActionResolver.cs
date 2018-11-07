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
    internal static class SubmitActionResolver
    {
        /// <summary>
        /// Resolve the Action
        /// </summary>
        /// <param name="submitMessage"></param>
        /// <returns></returns>
        public static string ResolveAction(SubmitMessage submitMessage)
        {
            if (submitMessage == null)
            {
                throw new ArgumentNullException(nameof(submitMessage));
            }

            if (submitMessage.PMode == null)
            {
                throw new ArgumentNullException(nameof(submitMessage.PMode));
            }

            SendingProcessingMode sendingPMode = submitMessage.PMode;
            string submitAction = submitMessage.Collaboration?.Action;
            string pmodeAction = submitMessage.PMode.MessagePackaging?.CollaborationInfo?.Action;

            if (sendingPMode.AllowOverride == false
                && !String.IsNullOrEmpty(submitAction)
                && !String.IsNullOrEmpty(pmodeAction)
                && !StringComparer.OrdinalIgnoreCase.Equals(submitAction, pmodeAction))
            {
                throw new InvalidOperationException(
                    $"SubmitMessage is not allowed by SendingPMode {sendingPMode.Id} to override Action");
            }

            if (!String.IsNullOrEmpty(submitAction))
            {
                return submitMessage.Collaboration.Action;
            }

            return PModeActionResolver.ResolveAction(sendingPMode);
        }
    }
}