using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using AgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;
using Service = Eu.EDelivery.AS4.Model.Core.Service;

namespace Eu.EDelivery.AS4.Mappings.PMode
{
    /// <summary>
    /// Collection of mapping functions from ebMS models to models in the <see cref="PMode"/> namespace.
    /// </summary>
    internal static class SendingPModeMap
    {
        /// <summary>
        /// Creates an <see cref="UserMessage"/> entirely from the given <paramref name="sendingPMode"/> information.
        /// </summary>
        /// <param name="sendingPMode">
        ///     The pmode from which the values in the <see cref="Model.PMode.SendingProcessingMode.MessagePackaging"/> will be used to create an <see cref="UserMessage"/>.
        /// </param>
        /// <param name="parts">The optional list of part references for attachments in the <see cref="AS4Message"/>.</param>
        internal static UserMessage CreateUserMessage(Model.PMode.SendingProcessingMode sendingPMode, params PartInfo[] parts)
        {
            if (sendingPMode == null)
            {
                throw new ArgumentNullException(nameof(sendingPMode));
            }

            if (parts == null)
            {
                throw new ArgumentNullException(nameof(parts));
            }

            IEnumerable<MessageProperty> properties =
                sendingPMode.MessagePackaging?.MessageProperties == null
                    ? Enumerable.Empty<MessageProperty>()
                    : sendingPMode.MessagePackaging.MessageProperties.Select(
                        p => new MessageProperty(p.Name, p.Value, p.Type));

            return new UserMessage(
                IdentifierFactory.Instance.Create(),
                sendingPMode.MessagePackaging?.Mpc,
                new CollaborationInfo(
                    ResolveAgreementReference(sendingPMode),
                    ResolveService(sendingPMode),
                    ResolveAction(sendingPMode),
                    CollaborationInfo.DefaultConversationId),
                ResolveSender(sendingPMode.MessagePackaging?.PartyInfo?.FromParty),
                ResolveReceiver(sendingPMode.MessagePackaging?.PartyInfo?.ToParty),
                parts,
                properties);
        }

        /// <summary>
        /// Resolves the <see cref="AgreementReference"/> from the <see cref="Model.PMode.SendingProcessingMode.MessagePackaging"/> element.
        /// </summary>
        /// <param name="pmode">The pmode to retrieve the agreement reference from.</param>
        internal static Maybe<AgreementReference> ResolveAgreementReference(Model.PMode.SendingProcessingMode pmode)
        {
            var pmodeAgreement =
                pmode?.MessagePackaging
                     ?.CollaborationInfo
                     ?.AgreementReference;

            string value = pmodeAgreement?.Value;
            if (value == null)
            {
                return Maybe<AgreementReference>.Nothing;
            }

            Maybe<string> type =
                (pmodeAgreement?.Type != null)
                .ThenMaybe(pmodeAgreement?.Type);

            Maybe<string> pmodeId =
                (pmodeAgreement?.PModeId != null)
                .ThenMaybe(pmodeAgreement?.PModeId)
                .Where(_ => pmode.MessagePackaging.IncludePModeId);

            return Maybe.Just(new AgreementReference(value, type, pmodeId));
        }

        /// <summary>
        /// Resolves the <see cref="Service"/> from the <see cref="Model.PMode.SendingProcessingMode.MessagePackaging"/> element.
        /// </summary>
        /// <param name="pmode">The pmode to retrieve the service from.</param>
        internal static Service ResolveService(Model.PMode.SendingProcessingMode pmode)
        {
            if (pmode?.MessagePackaging?.CollaborationInfo?.Service != null)
            {
                var pmodeService = pmode.MessagePackaging.CollaborationInfo.Service;
                if (String.IsNullOrEmpty(pmodeService.Value))
                {
                    return Service.TestService;
                }

                if (pmodeService.Type == null)
                {
                    return new Service(pmodeService.Value);
                }

                return new Service(pmodeService.Value, pmodeService.Type);
            }

            return Service.TestService;
        }

        /// <summary>
        /// Resolves the Action from the  <see cref="Model.PMode.SendingProcessingMode.MessagePackaging"/> element. 
        /// </summary>
        /// <param name="pmode">The pmode to retrieve the action from.</param>
        internal static string ResolveAction(Model.PMode.SendingProcessingMode pmode)
        {
            var pmodeCollaboration = pmode?.MessagePackaging?.CollaborationInfo;

            if (String.IsNullOrEmpty(pmodeCollaboration?.Action))
            {
                return Constants.Namespaces.TestAction;
            }

            return pmodeCollaboration.Action;

        }

        /// <summary>
        /// Resolves the sender party or FromParty from the <see cref="Model.PMode.Party"/> element.
        /// </summary>
        /// <param name="party">The party of the pmode to map to an ebMS party.</param>
        internal static Party ResolveSender(Model.PMode.Party party)
        {
            return party != null ? CreatePartyModel(party) : Party.DefaultFrom;
        }

        /// <summary>
        /// Resolves the receiver party or ToParty from the <see cref="Model.PMode.Party"/> element.
        /// </summary>
        /// <param name="party">The party of the pmode to map to an ebMS party.</param>
        internal static Party ResolveReceiver(Model.PMode.Party party)
        {
            return party != null ? CreatePartyModel(party) : Party.DefaultTo;
        }

        private static Party CreatePartyModel(Model.PMode.Party p)
        {
            var ids = 
                p.PartyIds == null
                    ? Enumerable.Empty<PartyId>()
                    : p.PartyIds?.Select(id => 
                        String.IsNullOrEmpty(id.Type)
                            ? new PartyId(id.Id)
                            : new PartyId(id.Id, id.Type));

            return new Party(p.Role, ids);
        }
    }
}
