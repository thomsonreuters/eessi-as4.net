using AutoMapper;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// Resolve <see cref="Party"/>
    /// </summary>
    public class SubmitReceiverPartyResolver : ISubmitResolver<Party>
    {
        /// <summary>
        /// Resolve <see cref="Party"/>
        /// 1. SubmitMessage / PartyInfo / ToParty
        /// 2. PMode / Message Packaging / PartyInfo / Toparty
        /// 3. Default
        /// </summary>
        /// <param name="submitMessage"></param>
        /// <returns></returns>
        public Party Resolve(SubmitMessage submitMessage)
        {
            PreConditionAllowOverride(submitMessage);

            if (IsSubmitMessageToPartyNotNull(submitMessage))
                return MapToPartyFromSubmitMessage(submitMessage);

            if (IsPModeToPartyNotNull(submitMessage))
                return Mapper.Map<Party>(submitMessage.PMode.MessagePackaging.PartyInfo.ToParty);

            return CreateDefaultParty();
        }

        private bool IsSubmitMessageToPartyNotNull(SubmitMessage submitMessage)
        {
            return submitMessage?.PartyInfo?.ToParty != null;
        }

        private bool IsPModeToPartyNotNull(SubmitMessage submitMessage)
        {
            return submitMessage?.PMode.MessagePackaging.PartyInfo?.ToParty != null;
        }

        private Party MapToPartyFromSubmitMessage(SubmitMessage message)
        {
            var toParty = Mapper.Map<Party>(message.PartyInfo.ToParty);
            // AutoMapper doens't map "Role"
            toParty.Role = message.PartyInfo.ToParty.Role;

            return toParty;
        }

        private Party CreateDefaultParty()
        {
            var partyId = new PartyId {Id = Constants.Namespaces.EbmsDefaultTo};
            return new Party(partyId) {Role = Constants.Namespaces.EbmsDefaultRole};
        }

        private void PreConditionAllowOverride(SubmitMessage message)
        {
            if (message?.PartyInfo?.ToParty != null && message.PMode.AllowOverride == false)
                throw new AS4Exception(
                    $"Submit Message is not allowed by the Sending PMode {message.PMode.Id} to override Receiver Party");
        }
    }
}