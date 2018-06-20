using System.Linq;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Mappings.PMode
{
    /// <summary>
    /// Resolve the <see cref="Party"/> from the <see cref="Model.PMode.SendingProcessingMode"/>
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
        public Party Resolve(Model.PMode.SendingProcessingMode pmode)
        {
            Model.PMode.Party fromParty = pmode.MessagePackaging.PartyInfo?.FromParty;
            bool isToPartyPresent = fromParty != null;
            if (isToPartyPresent)
            {
                return new Party(
                    fromParty.Role, 
                    fromParty.PartyIds?.Select(id => new PartyId(id.Id, id.Type)));
            }

            return new Party(
                Constants.Namespaces.EbmsDefaultRole, 
                new PartyId(Constants.Namespaces.EbmsDefaultFrom));
        }
    }
}