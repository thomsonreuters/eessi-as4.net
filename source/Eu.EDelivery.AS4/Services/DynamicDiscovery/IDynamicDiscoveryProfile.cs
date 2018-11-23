using System;
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
        /// Retrieves the SMP meta data <see cref="XmlDocument"/> for a given <paramref name="party"/> using a given <paramref name="properties"/>.
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="properties"></param>
        Task<XmlDocument> RetrieveSmpMetaDataAsync(Model.Core.Party party, IDictionary<string, string> properties);

        /// <summary>
        /// Complete the <paramref name="pmode"/> with the SMP metadata that is present in the <paramref name="smpMetaData"/> <see cref="XmlDocument"/>
        /// </summary>
        /// <param name="pmode">The <see cref="SendingProcessingMode"/> that must be decorated with the SMP metadata</param>
        /// <param name="smpMetaData">An XmlDocument that contains the SMP MetaData that has been received from an SMP server.</param>
        /// <returns>The completed <see cref="SendingProcessingMode"/></returns>
        DynamicDiscoveryResult DecoratePModeWithSmpMetaData(SendingProcessingMode pmode, XmlDocument smpMetaData);
    }

    /// <summary>
    /// Contract which is the result of a dynamic discovery operation.
    /// </summary>
    public class DynamicDiscoveryResult
    {
        /// <summary>
        /// The complete dynamically discovered <see cref="SendingProcessingMode"/> model.
        /// </summary>
        public SendingProcessingMode CompletedSendingPMode { get; }

        /// <summary>
        /// Whether or not the ToParty should be overriden in the <see cref="Model.Submit.SubmitMessage"/>.
        /// </summary>
        public bool OverrideToParty { get; }

        private DynamicDiscoveryResult(SendingProcessingMode pmode, bool overrideToParty)
        {
            CompletedSendingPMode = pmode;
            OverrideToParty = overrideToParty;
        }

        /// <summary>
        /// Creates a <see cref="DynamicDiscoveryResult"/> based on a given <paramref name="sendingPMode"/>.
        /// </summary>
        /// <param name="sendingPMode">The pmode for which the dynamic discovery has happened.</param>
        /// <param name="overrideToParty">The value indicating whether or not the ToParty should be overriden in the <see cref="Model.Submit.SubmitMessage"/>.</param>
        public static DynamicDiscoveryResult Create(
            SendingProcessingMode sendingPMode, 
            bool overrideToParty = false)
        {
            if (sendingPMode == null)
            {
                throw new ArgumentNullException(nameof(sendingPMode));
            }

            return new DynamicDiscoveryResult(sendingPMode, overrideToParty);
        }
    }
}
