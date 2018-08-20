using System.Linq;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Mappings.PMode
{
    /// <summary>
    /// Resolve the <see cref="Party"/> from the <see cref="Model.PMode.SendingProcessingMode"/>
    /// </summary>
    public class PModePartyResolver
    {
        /// <summary>
        /// Resolve the sending <see cref="Party"/>
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Party ResolveSender(Model.PMode.Party p)
        {
            return p != null ? CreatePartyModel(p) : Party.DefaultFrom;
        }

        /// <summary>
        /// Resolve the receiving <see cref="Party"/>
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Party ResolveReceiver(Model.PMode.Party p)
        {
            return p != null ? CreatePartyModel(p) : Party.DefaultTo;
        }

        private static Party CreatePartyModel(Model.PMode.Party p)
        {
            var ids = p.PartyIds?.Select(
                id => string.IsNullOrEmpty(id.Type)
                    ? new PartyId(id.Id)
                    : new PartyId(id.Id, id.Type));

            return new Party(p.Role, ids);
        }
    }
}