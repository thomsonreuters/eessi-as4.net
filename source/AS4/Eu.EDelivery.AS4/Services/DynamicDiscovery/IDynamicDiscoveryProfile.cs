using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Services.DynamicDiscovery
{
    /// <summary>
    /// Defines how SMP/SML Dynamic Discovery should be executed in a given profile.
    /// </summary>
    public interface IDynamicDiscoveryProfile
    {
        /// <summary>
        /// Retrieves the SMP meta data <see cref="XmlDocument"/> for a given <paramref name="partyId"/> using a given <paramref name="config"/>.
        /// </summary>
        /// <param name="partyId">The party identifier.</param>
        /// <param name="properties"></param>
        /// <returns></returns>
        Task<XmlDocument> RetrieveSmpMetaData(string partyId, IDictionary<string, string> properties);

        /// <summary>
        /// Complete the <paramref name="pmode"/> with the SMP metadata that is present in the <paramref name="smpMetaData"/> <see cref="XmlDocument"/>
        /// </summary>
        /// <param name="pmode">The <see cref="SendingProcessingMode"/> that must be decorated with the SMP metadata</param>
        /// <param name="smpMetaData">An XmlDocument that contains the SMP MetaData that has been received from an SMP server.</param>
        /// <returns>The completed <see cref="SendingProcessingMode"/></returns>
        SendingProcessingMode DecoratePModeWithSmpMetaData(SendingProcessingMode pmode, XmlDocument smpMetaData);
    }
}
