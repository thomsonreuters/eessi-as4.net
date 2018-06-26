using System;
using Eu.EDelivery.AS4.Model.PMode;
using Service = Eu.EDelivery.AS4.Model.Core.Service;

namespace Eu.EDelivery.AS4.Mappings.PMode
{
    /// <summary>
    /// Resolve the <see cref="Service"/> 
    /// </summary>
    public class PModeServiceResolver : IPModeResolver<Service>
    {
        /// <summary>
        /// 2. PMode / Message Packaging / CollaborationInfo / Service
        /// </summary>
        /// <param name="pmode"></param>
        /// <returns></returns>
        public Service Resolve(SendingProcessingMode pmode)
        {
            if (pmode.MessagePackaging?.CollaborationInfo?.Service != null)
            {
                var pmodeService = pmode.MessagePackaging.CollaborationInfo.Service;
                if (String.IsNullOrEmpty(pmodeService.Value))
                {
                    return Service.TestService;
                }

                if (pmodeService.Type == null)
                {
                    return new Service(pmodeService.Value);
                }

                return new Service(pmodeService.Value, pmodeService.Type);
            }

            return Service.TestService;
        }
    }
}
