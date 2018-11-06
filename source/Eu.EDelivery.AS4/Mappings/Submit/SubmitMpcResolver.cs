using System;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// 1. SubmitMessage / MessageInfo / Mpc
    /// 2. PMode / Message Packaging / Mpc
    /// 3. No mpc attribute
    /// </summary>
    internal static class SubmitMpcResolver
    {
        /// <summary>
        /// Resolve the Mpc from the <paramref name="submitMessage"/>
        /// </summary>
        /// <param name="submitMessage"></param>
        /// <returns></returns>
        public static string Resolve(SubmitMessage submitMessage)
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
            string pmodeMpc = sendingPMode.MessagePackaging?.Mpc;
            string submitMpc = submitMessage.MessageInfo?.Mpc;

            if (sendingPMode.AllowOverride == false
                && !String.IsNullOrEmpty(submitMpc)
                && !StringComparer.OrdinalIgnoreCase.Equals(Constants.Namespaces.EbmsDefaultMpc, submitMpc)
                && !String.IsNullOrEmpty(pmodeMpc)
                && !StringComparer.OrdinalIgnoreCase.Equals(submitMpc, pmodeMpc))
            {
                throw new InvalidOperationException(
                    $"SubmitMessage is not allowed by SendingPMode {sendingPMode.Id} to override Mpc");
            }

            if (!String.IsNullOrEmpty(submitMpc))
            {
                return submitMpc;
            }

            if (!String.IsNullOrEmpty(pmodeMpc))
            {
                return pmodeMpc;
            }

            return Constants.Namespaces.EbmsDefaultMpc;
        }
    }
}
