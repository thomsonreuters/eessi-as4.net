using System;
using System.Collections.Generic;
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
    public static class SubmitReceiverResolver
    {
        /// <summary>
        /// Resolve <see cref="Party" />
        /// 1. SubmitMessage / PartyInfo / ToParty
        /// 2. PMode / Message Packaging / PartyInfo / Toparty
        /// 3. Default
        /// </summary>
        /// <param name="submit"></param>
        /// <returns></returns>
        public static Party ResolveReceiver(SubmitMessage submit)
        {
            var submitParty = submit.PartyInfo?.ToParty;
            var pmodeParty = submit.PMode?.MessagePackaging?.PartyInfo?.ToParty;

            if (submitParty != null && submit?.PMode?.AllowOverride == false)
            {
                var messagePartyInfo = submitParty.PartyIds.Select(p => p.Id).OrderBy(p => p);
                var pmodePartyInfo = pmodeParty?.PartyIds?.Select(p => p.Id).OrderBy(p => p);

                if (pmodePartyInfo != null && messagePartyInfo.SequenceEqual(pmodePartyInfo) == false)
                {
                    throw new NotSupportedException(
                        $"Submit Message is not allowed by the Sending PMode {submit.PMode?.Id} to override Receiver Party");
                }
            }

            return submitParty != null 
                ? AS4Mapper.Map<Party>(submit.PartyInfo.ToParty) 
                : PModePartyResolver.ResolveReceiver(pmodeParty);
        }
    }
}