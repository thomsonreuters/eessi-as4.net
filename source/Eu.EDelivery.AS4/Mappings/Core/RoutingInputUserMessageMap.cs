using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Xml;
using static Eu.EDelivery.AS4.Singletons.AS4Mapper;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using PartInfo = Eu.EDelivery.AS4.Model.Core.PartInfo;
using Service = Eu.EDelivery.AS4.Model.Core.Service;
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
                        messageId: xml.MessageInfo?.MessageId,
                        refToMessageId: xml.MessageInfo?.RefToMessageId,
                        timestamp: xml?.MessageInfo?.Timestamp ?? DateTimeOffset.Now,
                        mpc: String.IsNullOrEmpty(xml.mpc) ? Constants.Namespaces.EbmsDefaultMpc : xml.mpc,
                        collaboration: RemoveResponsePostfixToActionWhenEmpty(Map<CollaborationInfo>(xml.CollaborationInfo)),
                        sender: Map<Party>(xml.PartyInfo?.From),
                        receiver: Map<Party>(xml.PartyInfo?.To),
                        partInfos: MapPartInfos(xml.PayloadInfo),
                        messageProperties: MapMessageProperties(xml.MessageProperties)))
                .ForAllOtherMembers(m => m.Ignore());
        }

        private static CollaborationInfo RemoveResponsePostfixToActionWhenEmpty(CollaborationInfo mapped)
        {
            if (mapped == null)
            {
                return new CollaborationInfo(
                    Maybe<AgreementReference>.Nothing,
                    Service.TestService,
                    Constants.Namespaces.TestAction,
                    CollaborationInfo.DefaultConversationId);
            }

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

        private static IEnumerable<PartInfo> MapPartInfos(Xml.PartInfo[] parts)
        {
            if (parts == null || !parts.Any())
            {
                return new Model.Core.PartInfo[0];
            }

            return parts.Select(Map<Model.Core.PartInfo>).Where(p => p != null);
        }

        private static IEnumerable<Model.Core.MessageProperty> MapMessageProperties(Xml.Property[] props)
        {
            if (props == null)
            {
                return Enumerable.Empty<Model.Core.MessageProperty>();
            }

            return props.Where(p => p != null)
                        .Select(Map<Model.Core.MessageProperty>)
                        .Where(p => p != null);
        }
    }
}
