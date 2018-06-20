using System.Linq;
using Eu.EDelivery.AS4.Model.PMode;
using Party = Eu.EDelivery.AS4.Model.Core.Party;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;

namespace Eu.EDelivery.AS4.Mappings.PMode
{
    /// <summary>
    /// Resolve the Receiver <see cref="Model.Core.Party" /> from the <see cref="Model.PMode.SendingProcessingMode" />
    /// </summary>
    public class PModeReceiverResolver : IPModeResolver<Party>
    {
        public static readonly PModeReceiverResolver Default = new PModeReceiverResolver();

        /// <summary>
        /// 2. PMode / Message Packaging / PartyInfo / Toparty
        /// </summary>
        /// <param name="pmode"></param>
        /// <returns></returns>
        public Party Resolve(SendingProcessingMode pmode)
        {
            Model.PMode.Party toParty = pmode.MessagePackaging.PartyInfo?.ToParty;
            bool isToPartyPresent = toParty != null;
            if (isToPartyPresent)
            {
                return new Party(
                    toParty.Role, 
                    toParty.PartyIds?.Select(id => new PartyId(id.Id, id.Type)));
            }

            return new Party(
                Constants.Namespaces.EbmsDefaultRole,
                new PartyId(Constants.Namespaces.EbmsDefaultTo));
        }
    }
}