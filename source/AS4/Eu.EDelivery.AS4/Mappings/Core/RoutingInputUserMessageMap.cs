using System;
using AutoMapper;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Xml;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
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
                .ForMember(dest => dest.Mpc, src => src.MapFrom(t => t.mpc))
                .ForMember(dest => dest.CollaborationInfo, src => src.MapFrom(t => t.CollaborationInfo))
                .ForMember(dest => dest.PayloadInfo, src => src.MapFrom(t => t.PayloadInfo))
                .ForMember(dest => dest.MessageProperties, src => src.MapFrom(t => t.MessageProperties))
                .AfterMap(
                    (routingInput, userMessage) =>
                    {
                        if (routingInput.PartyInfo != null)
                        {
                            userMessage.Sender = AS4Mapper.Map<Party>(routingInput.PartyInfo.From);
                            userMessage.Receiver = AS4Mapper.Map<Party>(routingInput.PartyInfo.To); 
                        }

                        AssignAction(userMessage);
                        AssignMpc(userMessage);

                    })
                    .ForAllOtherMembers(m => m.Ignore());            
        }

        private static void AssignAction(UserMessage userMessage)
        {
            string action = userMessage.CollaborationInfo?.Action;

            if (!String.IsNullOrWhiteSpace(action) && action.EndsWith(".response", StringComparison.OrdinalIgnoreCase))
            {
                userMessage.CollaborationInfo = new CollaborationInfo(
                    userMessage.CollaborationInfo.AgreementReference,
                    userMessage.CollaborationInfo.Service,
                    action.Substring(0, action.LastIndexOf(".response", StringComparison.OrdinalIgnoreCase)),
                    userMessage.CollaborationInfo.ConversationId);
            }
        }

        private static void AssignMpc(UserMessage userMessage)
        {
            if (string.IsNullOrEmpty(userMessage.Mpc))
            {
                userMessage.Mpc = Constants.Namespaces.EbmsDefaultMpc;
            }
        }
    }
}
