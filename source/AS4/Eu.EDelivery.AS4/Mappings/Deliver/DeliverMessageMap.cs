using AutoMapper;
using Eu.EDelivery.AS4.Singletons;

namespace Eu.EDelivery.AS4.Mappings.Deliver
{
    /// <summary>
    /// Map from a <see cref="Model.Core.UserMessage"/> to a <see cref="Model.Deliver.DeliverMessage"/>
    /// </summary>
    public class DeliverMessageMap : Profile
    {
        public DeliverMessageMap()
        {
            CreateMap<Model.Core.UserMessage, Model.Deliver.DeliverMessage>()
                .ForMember(dest => dest.CollaborationInfo, src => src.MapFrom(x => x.CollaborationInfo))
                .ForMember(dest => dest.Payloads, src => src.MapFrom(x => x.PayloadInfo))
                .ForMember(dest => dest.MessageProperties, src => src.MapFrom(x => x.MessageProperties))
                .AfterMap((userMessage, deliverMessage) =>
                {
                    deliverMessage.MessageInfo.MessageId = userMessage.MessageId;
                    deliverMessage.MessageInfo.RefToMessageId = userMessage.RefToMessageId;
                    deliverMessage.MessageInfo.Mpc = userMessage.Mpc ?? string.Empty;

                    deliverMessage.PartyInfo.FromParty = AS4Mapper.Map<Model.Common.Party>(userMessage.Sender);
                    deliverMessage.PartyInfo.ToParty = AS4Mapper.Map<Model.Common.Party>(userMessage.Receiver);

                    deliverMessage.CollaborationInfo.ConversationId = userMessage.CollaborationInfo.ConversationId;
                    deliverMessage.CollaborationInfo.Action = userMessage.CollaborationInfo.Action;

                    deliverMessage.CollaborationInfo.Service = deliverMessage.CollaborationInfo.Service ?? new Model.Common.Service();
                    deliverMessage.CollaborationInfo.Service.Value = userMessage.CollaborationInfo.Service?.Value;
                    deliverMessage.CollaborationInfo.Service.Type = userMessage.CollaborationInfo.Service?.Type;
                })
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}