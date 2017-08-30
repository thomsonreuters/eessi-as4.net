using System;
using System.Linq;
using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Singletons;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// Resolve <see cref="Party" />
    /// </summary>
    public class SubmitReceiverResolver : ISubmitResolver<Party>
    {
        private readonly IPModeResolver<Party> _pmodeResolver;

        public static readonly SubmitReceiverResolver Default = new SubmitReceiverResolver();

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitReceiverResolver" /> class
        /// </summary>
        private SubmitReceiverResolver()
        {
            _pmodeResolver = new PModeReceiverResolver();
        }

        /// <summary>
        /// Resolve <see cref="Party" />
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
            {
                return MapToPartyFromSubmitMessage(submitMessage);
            }

            return _pmodeResolver.Resolve(submitMessage.PMode);
        }

        private static bool IsSubmitMessageToPartyNotNull(SubmitMessage submitMessage)
        {
            return submitMessage?.PartyInfo?.ToParty != null;
        }

        private static Party MapToPartyFromSubmitMessage(SubmitMessage message)
        {
            var toParty = AS4Mapper.Map<Party>(message.PartyInfo.ToParty);

            // AutoMapper doens't map "Role"
            toParty.Role = message.PartyInfo.ToParty.Role;

            return toParty;
        }

        private static void PreConditionAllowOverride(SubmitMessage message)
        {
            if (message?.PartyInfo?.ToParty != null && message.PMode.AllowOverride == false)
            {
                // If the ToPartyInfo is equal as the ToParty information in the PMode, then there is no problem.
                IOrderedEnumerable<string> messagePartyInfo =
                    message.PartyInfo.ToParty.PartyIds.Select(p => p.Id).OrderBy(p => p);

                IOrderedEnumerable<string> pmodePartyInfo =
                    message.PMode.MessagePackaging?.PartyInfo?.ToParty?.PartyIds?.Select(p => p.Id).OrderBy(p => p);

                if (pmodePartyInfo != null && messagePartyInfo.SequenceEqual(pmodePartyInfo) == false)
                {
                    throw new NotSupportedException(
                        $"Submit Message is not allowed by the Sending PMode {message.PMode.Id} to override Receiver Party");
                }
            }
        }
    }
}