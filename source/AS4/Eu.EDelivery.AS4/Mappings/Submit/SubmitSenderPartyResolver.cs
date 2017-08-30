using System;
using System.Linq;
using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Singletons;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// Resolve the <see cref="Party" />
    /// </summary>
    public class SubmitSenderPartyResolver : ISubmitResolver<Party>
    {
        private readonly IPModeResolver<Party> _pmodeResolver;

        public static readonly SubmitSenderPartyResolver Default = new SubmitSenderPartyResolver();

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitSenderPartyResolver" /> class
        /// </summary>
        public SubmitSenderPartyResolver()
        {
            _pmodeResolver = PModeSenderResolver.Default;
        }

        /// <summary>
        /// Resolve <see cref="Party" />
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
            {
                return MapPartyFromSubmitMessage(submitMessage);
            }

            return _pmodeResolver.Resolve(submitMessage.PMode);
        }

        private static bool IsSubmitMessageFromPartyNotNull(SubmitMessage submitMessage)
        {
            return submitMessage?.PartyInfo?.FromParty != null;
        }

        private static Party MapPartyFromSubmitMessage(SubmitMessage submitMessage)
        {
            var fromParty = AS4Mapper.Map<Party>(submitMessage.PartyInfo.FromParty);

            // AutoMapper doesn't map "Role"
            fromParty.Role = submitMessage.PartyInfo.FromParty.Role;

            return fromParty;
        }

        private static void PreCoditionParty(SubmitMessage s)
        {
            if (s?.PartyInfo?.FromParty != null && s.PMode.AllowOverride == false)
            {
                IOrderedEnumerable<string> messagePartyInfo =
                    s.PartyInfo.FromParty.PartyIds.Select(p => p.Id).OrderBy(p => p);

                IOrderedEnumerable<string> pmodePartyInfo =
                    s.PMode.MessagePackaging.PartyInfo.FromParty.PartyIds.Select(p => p.Id).OrderBy(p => p);

                if (messagePartyInfo.SequenceEqual(pmodePartyInfo) == false)
                {
                    throw new NotSupportedException(
                        $"Submit Message is not allowed by Sending PMode{s.PMode.Id} to override Sender Party");
                }
            }
        }
    }
}