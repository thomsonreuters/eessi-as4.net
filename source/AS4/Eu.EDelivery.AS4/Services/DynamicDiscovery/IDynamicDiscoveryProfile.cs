using System;
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
        /// Create the Uri to the SMP Server that must be contacted to GET the SMP Meta-Data
        /// </summary>
        /// <param name="partyId">The Id of the Party to where to send the Message</param>
        /// <param name="dynamicDiscoveryConfiguration">The <see cref="DynamicDiscoveryConfiguration"/></param> information that is 
        /// present in the <see cref="SendingProcessingMode"/>
        /// <returns></returns>
        Uri CreateSmpServerUri(string partyId, DynamicDiscoveryConfiguration dynamicDiscoveryConfiguration);

        /// <summary>
        /// Complete the <paramref name="pmode"/> with the SMP metadata that is present in the <paramref name="smpMetaData"/> <see cref="XmlDocument"/>
        /// </summary>
        /// <param name="pmode">The <see cref="SendingProcessingMode"/> that must be decorated with the SMP metadata</param>
        /// <param name="smpMetaData">An XmlDocument that contains the SMP MetaData that has been received from an SMP server.</param>
        /// <returns>The completed <see cref="SendingProcessingMode"/></returns>
        SendingProcessingMode DecoratePModeWithSmpMetaData(SendingProcessingMode pmode, XmlDocument smpMetaData);
    }
}
