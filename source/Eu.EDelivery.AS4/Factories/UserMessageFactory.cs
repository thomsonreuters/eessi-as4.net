using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using MessageProperty = Eu.EDelivery.AS4.Model.Core.MessageProperty;

namespace Eu.EDelivery.AS4.Factories
{
    /// <summary>
    /// Factory to create <see cref="UserMessage"/> Models
    /// </summary>
    public class UserMessageFactory
    {

        public static readonly UserMessageFactory Instance = new UserMessageFactory();

        /// <summary>
        /// Create default <see cref="UserMessage"/>
        /// </summary>
        /// <returns></returns>
        public UserMessage Create(SendingProcessingMode pmode)
        {
            if (pmode == null)
            {
                throw new ArgumentNullException(nameof(pmode));
            }

            IEnumerable<MessageProperty> properties =
                pmode.MessagePackaging?.MessageProperties?.Select(
                    p => new MessageProperty(p.Name, p.Value, p.Type)) ?? new MessageProperty[0];

            return new UserMessage(
                IdentifierFactory.Instance.Create(),
                pmode.MessagePackaging?.Mpc ?? Constants.Namespaces.EbmsDefaultMpc,
                ResolveCollaborationInfo(pmode),
                PModePartyResolver.ResolveSender(pmode.MessagePackaging?.PartyInfo?.FromParty),
                PModePartyResolver.ResolveReceiver(pmode.MessagePackaging?.PartyInfo?.ToParty),
                new PartInfo[0], 
                properties);
        }

        private static CollaborationInfo ResolveCollaborationInfo(SendingProcessingMode pmode)
        {
            return new CollaborationInfo(
                PModeAgreementRefResolver.ResolveAgreementReference(pmode),
                PModeServiceResolver.ResolveService(pmode),
                PModeActionResolver.ResolveAction(pmode),
                CollaborationInfo.DefaultConversationId);
        }
    }
}
