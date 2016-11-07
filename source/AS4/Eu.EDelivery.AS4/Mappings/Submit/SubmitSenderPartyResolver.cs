using AutoMapper;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// Resolve the <see cref="Party"/>
    /// </summary>
    public class SubmitSenderPartyResolver : ISubmitResolver<Party>
    {
        /// <summary>
        /// Resolve <see cref="Party"/>
        /// 1. SubmitMessage / PartyInfo / FromParty
        /// 2. PMode / Message Packaging / PartyInfo / FromParty
        /// 3. Default
        /// </summary>
        /// <param name="submitMessage"></param>
        /// <returns></returns>
        public Party Resolve(SubmitMessage submitMessage)
        {
            PreCoditionParty(submitMessage);

            if (IsSubmitMessageFromPartyNotNull(submitMessage))
                return MapPartyFromSubmitMessage(submitMessage);

            if (IsPModeFromPartyNotNull(submitMessage))
                return Mapper.Map<Party>(submitMessage.PMode.MessagePackaging.PartyInfo.FromParty);

            return CreateDefaultParty();
        }

        private bool IsPModeFromPartyNotNull(SubmitMessage submitMessage)
        {
            return submitMessage?.PMode.MessagePackaging.PartyInfo?.FromParty != null;
        }

        private bool IsSubmitMessageFromPartyNotNull(SubmitMessage submitMessage)
        {
            return submitMessage?.PartyInfo?.FromParty != null;
        }

        private Party CreateDefaultParty()
        {
            var partyId = new PartyId {Id = Constants.Namespaces.EbmsDefaultFrom};
            return new Party(partyId) {Role = Constants.Namespaces.EbmsDefaultRole};
        }

        private Party MapPartyFromSubmitMessage(SubmitMessage submitMessage)
        {
            var fromParty = Mapper.Map<Party>(submitMessage.PartyInfo.FromParty);
            // AutoMapper doesn't map "Role"
            fromParty.Role = submitMessage.PartyInfo.FromParty.Role;

            return fromParty;
        }

        private void PreCoditionParty(SubmitMessage s)
        {
            if (s?.PartyInfo?.FromParty != null && s.PMode.AllowOverride == false)
                throw new AS4Exception(
                    $"Submit Message is not allowed by Sending PMode{s.PMode.Id} to override Sender Party");
        }
    }
}