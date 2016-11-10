using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

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
                return pmode.MessagePackaging.CollaborationInfo.Service;

            return GetDefaultService();
        }

        private Service GetDefaultService()
        {
            return new Service();
        }
    }
}
