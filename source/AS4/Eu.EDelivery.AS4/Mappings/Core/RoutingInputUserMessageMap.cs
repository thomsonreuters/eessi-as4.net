using System;
using System.Collections.Generic;
using AutoMapper;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Xml;
using static Eu.EDelivery.AS4.Singletons.AS4Mapper;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using PartInfo = Eu.EDelivery.AS4.Model.Core.PartInfo;
using UserMessage = Eu.EDelivery.AS4.Model.Core.UserMessage;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class RoutingInputUserMessageMap : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoutingInputUserMessageMap"/> class.
        /// </summary>
        public RoutingInputUserMessageMap()
        {
            CreateMap<RoutingInputUserMessage, UserMessage>()
                .ConstructUsing(xml => 
                    new UserMessage(
                        messageId: String.IsNullOrEmpty(xml.mpc) ? Constants.Namespaces.EbmsDefaultMpc : xml.mpc,
                        collaboration: RemoveResponsePostfixToActionWhenEmpty(Map<CollaborationInfo>(xml.CollaborationInfo)),
                        sender: Map<Party>(xml.PartyInfo.From),
                        receiver: Map<Party>(xml.PartyInfo.To),
                        partInfos: Map<IEnumerable<PartInfo>>(xml.PayloadInfo),
                        messageProperties: Map<IEnumerable<MessageProperty>>(xml.MessageProperties)))
                .ForAllOtherMembers(m => m.Ignore());
        }

        private static CollaborationInfo RemoveResponsePostfixToActionWhenEmpty(CollaborationInfo mapped)
        {
            string action = mapped.Action;
            if (!String.IsNullOrWhiteSpace(action)
                && action.EndsWith(".response", StringComparison.OrdinalIgnoreCase))
            {
                return new CollaborationInfo(
                    mapped.AgreementReference,
                    mapped.Service,
                    action.Substring(0, action.LastIndexOf(".response", StringComparison.OrdinalIgnoreCase)),
                    mapped.ConversationId);
            }

            return mapped;
        }
    }
}
