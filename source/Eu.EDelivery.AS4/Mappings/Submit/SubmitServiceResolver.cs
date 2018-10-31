using System;
using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Service = Eu.EDelivery.AS4.Model.Core.Service;
using SubmitService = Eu.EDelivery.AS4.Model.Common.Service;
using PModeService = Eu.EDelivery.AS4.Model.PMode.Service;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// Resolve the <see cref="Service"/>
    /// </summary>
    internal class SubmitServiceResolver
    {
        /// <summary>
        /// 1. SubmitMessage / CollaborationInfo / Service / Value
        /// 2. PMode / Message Packaging / CollaborationInfo / Service / Value
        /// 3. Default
        /// </summary>
        /// <param name="submitMessage"></param>
        /// <returns></returns>
        public static Service ResolveService(SubmitMessage submitMessage)
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
            PModeService pmodeService = sendingPMode.MessagePackaging?.CollaborationInfo?.Service;
            SubmitService submitService = submitMessage.Collaboration?.Service;

            if (sendingPMode.AllowOverride == false
                && !String.IsNullOrEmpty(submitService?.Value)
                && !String.IsNullOrEmpty(pmodeService?.Value)
                && !StringComparer.OrdinalIgnoreCase.Equals(submitService?.Value, pmodeService?.Value))
            {
                throw new NotSupportedException(
                    $"SubmitMessage is not allowed by SendingPMode {sendingPMode.Id} to override CollaborationInfo.Service");
            }

            if (submitService?.Value != null)
            {
                return submitService.Type != null 
                    ? new Service(submitService.Value, submitService.Type) 
                    : new Service(submitService.Value);
            }

            return PModeServiceResolver.ResolveService(sendingPMode);
        }
    }
}
