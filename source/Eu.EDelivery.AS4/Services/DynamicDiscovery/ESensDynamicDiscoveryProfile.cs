using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        [Info("SML Scheme", defaultValue: "iso6523-actorid-upis")]
        [Description("Used to build the SML Uri")]
        // Property is used to determine the configuration options via reflection
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        private string SmlScheme { get; }

        [Info("SMP Server Domain Name", defaultValue: "isaitb.acc.edelivery.tech.ec.europa.eu")]
        [Description("Domain name that must be used in the Uri")]
        // Property is used to determine the configuration options via reflection
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        private string SmpServerDomainName { get; }

        /// <summary>
        /// Retrieves the SMP meta data <see cref="XmlDocument"/> for a given <paramref name="party"/> using a given <paramref name="properties"/>.
        /// </summary>
        /// <param name="party">The party identifier to select the right SMP meta-data.</param>
        /// <param name="properties">The information properties specified in the <see cref="SendingProcessingMode"/> for this profile.</param>
        public Task<XmlDocument> RetrieveSmpMetaDataAsync(Model.Core.Party party, IDictionary<string, string> properties)
        {
            return PeppolDynamicDiscoveryProfile.RetrieveSmpMetaDataAsync(party, properties);
        }

        /// <summary>
        /// Complete the <paramref name="pmode"/> with the SMP metadata that is present in the <paramref name="smpMetaData"/> <see cref="XmlDocument"/>
        /// </summary>
        /// <param name="pmode">The <see cref="SendingProcessingMode"/> that must be decorated with the SMP metadata</param>
        /// <param name="smpMetaData">An XmlDocument that contains the SMP MetaData that has been received from an SMP server.</param>
        /// <returns>The completed <see cref="SendingProcessingMode"/></returns>
        public DynamicDiscoveryResult DecoratePModeWithSmpMetaData(SendingProcessingMode pmode, XmlDocument smpMetaData)
        {
            return PeppolDynamicDiscoveryProfile.DecoratePModeWithSmpMetaData(pmode, smpMetaData);
        }
    }
}