using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Mappings.PMode
{
    /// <summary>
    /// Resolve the Receiver <see cref="Party"/> from the <see cref="SendingProcessingMode"/>
    /// </summary>
    public class PModeReceiverResolver : IPModeResolver<Party>
    {
        /// <summary>
        /// 2. PMode / Message Packaging / PartyInfo / Toparty
        /// </summary>
        /// <param name="pmode"></param>
        /// <returns></returns>
        public Party Resolve(SendingProcessingMode pmode)
        {
            if (IsPModeToPartyNotNull(pmode))
                return pmode.MessagePackaging.PartyInfo.ToParty;

            return CreateDefaultParty();
        }

        private bool IsPModeToPartyNotNull(SendingProcessingMode pmode)
        {
            return pmode.MessagePackaging.PartyInfo?.ToParty != null;
        }

        private Party CreateDefaultParty()
        {
            var partyId = new PartyId {Id = Constants.Namespaces.EbmsDefaultTo};
            return new Party(partyId) {Role = Constants.Namespaces.EbmsDefaultRole};
        }
    }
}