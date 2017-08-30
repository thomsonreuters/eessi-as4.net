using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Mappings.PMode
{
    /// <summary>
    /// Resolve the <see cref="Party"/> from the <see cref="SendingProcessingMode"/>
    /// </summary>
    public class PModeSenderResolver : IPModeResolver<Party>
    {
        public static readonly PModeSenderResolver Default = new PModeSenderResolver();

        /// <summary>
        /// Initializes a new instance of the <see cref="PModeSenderResolver"/> class.
        /// </summary>
        private PModeSenderResolver()
        {
        }

        /// <summary>
        /// 2. PMode / Message Packaging / PartyInfo / FromParty
        /// </summary>
        /// <param name="pmode"></param>
        /// <returns></returns>
        public Party Resolve(SendingProcessingMode pmode)
        {
            if (IsPModeFromPartyNotNull(pmode))
            {
                return pmode.MessagePackaging.PartyInfo.FromParty;
            }

            return CreateDefaultParty();
        }

        private static bool IsPModeFromPartyNotNull(SendingProcessingMode pmode)
        {
            return pmode.MessagePackaging.PartyInfo?.FromParty != null;
        }

        private static Party CreateDefaultParty()
        {
            var partyId = new PartyId { Id = Constants.Namespaces.EbmsDefaultFrom };
            return new Party(partyId) { Role = Constants.Namespaces.EbmsDefaultRole };
        }
    }
}