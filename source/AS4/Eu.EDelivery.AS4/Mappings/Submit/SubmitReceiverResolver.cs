using System.Linq;
using AutoMapper;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Singletons;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// Resolve <see cref="Party"/>
    /// </summary>
    public class SubmitReceiverResolver : ISubmitResolver<Party>
    {
        private readonly IPModeResolver<Party> _pmodeResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitReceiverResolver"/> class
        /// </summary>
        public SubmitReceiverResolver()
        {
            this._pmodeResolver = new PModeReceiverResolver();
        }

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

            return this._pmodeResolver.Resolve(submitMessage.PMode);
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
                var messagePartyInfo = message.PartyInfo.ToParty.PartyIds.Select(p => p.Id).OrderBy(p => p);
                var pmodePartyInfo = message.PMode.MessagePackaging.PartyInfo.ToParty.PartyIds.Select(p=>p.Id).OrderBy(p => p);

                if (Enumerable.SequenceEqual(messagePartyInfo, pmodePartyInfo) == false)
                {
                    throw new AS4Exception(
$"Submit Message is not allowed by the Sending PMode {message.PMode.Id} to override Receiver Party");
                }

            }

        }
    }
}