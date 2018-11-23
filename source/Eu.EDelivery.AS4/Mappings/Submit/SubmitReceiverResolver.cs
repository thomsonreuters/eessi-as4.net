using System;
using System.Linq;
using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Party = Eu.EDelivery.AS4.Model.Core.Party;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;
using SubmitParty = Eu.EDelivery.AS4.Model.Common.Party;
using PModeParty = Eu.EDelivery.AS4.Model.PMode.Party;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// Resolve <see cref="Party" />
    /// </summary>
    internal static class SubmitReceiverResolver
    {
        /// <summary>
        /// Resolve <see cref="Model.Core.Party" />
        /// 1. SubmitMessage / PartyInfo / ToParty
        /// 2. PMode / Message Packaging / PartyInfo / ToParty
        /// 3. Default
        /// </summary>
        /// <param name="submit"></param>
        /// <returns></returns>
        public static Party ResolveReceiver(SubmitMessage submit)
        {
            if (submit == null)
            {
                throw new ArgumentNullException(nameof(submit));
            }

            if (submit.PMode == null)
            {
                throw new ArgumentNullException(nameof(submit.PMode));
            }

            SendingProcessingMode sendingPMode = submit.PMode;
            PModeParty pmodeParty = sendingPMode.MessagePackaging?.PartyInfo?.ToParty;
            SubmitParty submitParty = submit.PartyInfo?.ToParty;

            if (sendingPMode.AllowOverride == false
                && submitParty != null
                && pmodeParty != null
                && !submitParty.Equals(pmodeParty))
            {
                throw new NotSupportedException(
                    $"SubmitMessage is not allowed by the SendingPMode {sendingPMode.Id} to override Receiver Party");
            }

            if (submitParty != null)
            {
                return new Party(
                    submitParty.Role,
                    (submitParty.PartyIds ?? Enumerable.Empty<Model.Common.PartyId>())
                        ?.Select(id => id?.Type != null
                                     ? new PartyId(id?.Id, id?.Type) 
                                     : new PartyId(id?.Id))
                        .ToArray());
            }

            return SendingPModeMap.ResolveReceiver(pmodeParty);
        }
    }
}