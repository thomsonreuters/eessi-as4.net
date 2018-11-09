using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Services.DynamicDiscovery
{
    /// <summary>
    /// ESens Profile implementation for Dynamic Discovery
    /// </summary>
    /// <seealso cref="IDynamicDiscoveryProfile"/>
    [Obsolete("Renamed to " + nameof(PeppolDynamicDiscoveryProfile))]
    public class ESensDynamicDiscoveryProfile : IDynamicDiscoveryProfile
    {
        private static readonly IDynamicDiscoveryProfile PeppolDynamicDiscoveryProfile = new PeppolDynamicDiscoveryProfile();

        /// <summary>
        /// Retrieves the SMP meta data <see cref="XmlDocument"/> for a given <paramref name="party"/> using a given <paramref name="properties"/>.
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="properties"></param>
        public Task<XmlDocument> RetrieveSmpMetaData(Model.Core.Party party, IDictionary<string, string> properties)
        {
            return PeppolDynamicDiscoveryProfile.RetrieveSmpMetaData(party, properties);
        }

        /// <summary>
        /// Complete the <paramref name="pmode"/> with the SMP metadata that is present in the <paramref name="smpMetaData"/> <see cref="XmlDocument"/>
        /// </summary>
        /// <param name="pmode">The <see cref="SendingProcessingMode"/> that must be decorated with the SMP metadata</param>
        /// <param name="smpMetaData">An XmlDocument that contains the SMP MetaData that has been received from an SMP server.</param>
        /// <returns>The completed <see cref="SendingProcessingMode"/></returns>
        public SendingProcessingMode DecoratePModeWithSmpMetaData(SendingProcessingMode pmode, XmlDocument smpMetaData)
        {
            return PeppolDynamicDiscoveryProfile.DecoratePModeWithSmpMetaData(pmode, smpMetaData);
        }
    }
}