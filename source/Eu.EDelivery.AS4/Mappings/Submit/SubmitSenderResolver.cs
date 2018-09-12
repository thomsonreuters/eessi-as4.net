using System;
using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Singletons;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// Resolve the <see cref="Party" />
    /// </summary>
    public static class SubmitSenderResolver
    {
        /// <summary>
        /// Resolve <see cref="Party" />
        /// 1. SubmitMessage / PartyInfo / FromParty
        /// 2. PMode / Message Packaging / PartyInfo / FromParty
        /// 3. Default
        /// </summary>
        /// <param name="submit"></param>
        /// <returns></returns>
        public static Party ResolveSender(SubmitMessage submit)
        {
            var submitParty = submit.PartyInfo?.FromParty;
            var pmodeParty = submit.PMode.MessagePackaging?.PartyInfo?.FromParty;

            if (submitParty != null && submit.PMode.AllowOverride == false)
            {
                if (pmodeParty != null && submitParty.Equals(pmodeParty) == false)
                {
                    throw new NotSupportedException(
                        $"Submit Message is not allowed by Sending PMode{submit.PMode.Id} to override Sender Party");
                }
            }

            return submitParty != null 
                ? AS4Mapper.Map<Party>(submitParty) 
                : PModePartyResolver.ResolveSender(pmodeParty);
        }
    }
}