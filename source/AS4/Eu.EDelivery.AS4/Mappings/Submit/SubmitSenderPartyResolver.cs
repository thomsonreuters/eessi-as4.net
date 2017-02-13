using System.Linq;
using AutoMapper;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// Resolve the <see cref="Party"/>
    /// </summary>
    public class SubmitSenderPartyResolver : ISubmitResolver<Party>
    {
        private readonly IPModeResolver<Party> _pmodeResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitSenderPartyResolver"/> class
        /// </summary>
        public SubmitSenderPartyResolver()
        {
            this._pmodeResolver = new PModeSenderResolver();
        }

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

            return this._pmodeResolver.Resolve(submitMessage.PMode);
        }

        private bool IsSubmitMessageFromPartyNotNull(SubmitMessage submitMessage)
        {
            return submitMessage?.PartyInfo?.FromParty != null;
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
            {
                var messagePartyInfo = s.PartyInfo.FromParty.PartyIds.Select(p => p.Id).OrderBy(p => p);
                var pmodePartyInfo = s.PMode.MessagePackaging.PartyInfo.FromParty.PartyIds.Select(p => p.Id).OrderBy(p => p);

                if (Enumerable.SequenceEqual(messagePartyInfo, pmodePartyInfo) == false)
                {
                    throw new AS4Exception($"Submit Message is not allowed by Sending PMode{s.PMode.Id} to override Sender Party");
                }
            }

        }
    }
}