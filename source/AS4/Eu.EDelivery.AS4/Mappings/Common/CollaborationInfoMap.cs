using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Common
{
    public class CollaborationInfoMap : Profile
    {
        public CollaborationInfoMap()
        {
            CreateMap<Model.Common.CollaborationInfo, Model.Core.CollaborationInfo>()
                .ForMember(dest => dest.AgreementReference, src => src.MapFrom(s => s.AgreementRef))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Model.Core.CollaborationInfo, Model.Common.CollaborationInfo>()
                .ForMember(dest => dest.AgreementRef, src => src.MapFrom(x => x.AgreementReference))
                .ForMember(dest => dest.Action, src => src.MapFrom(x => x.Action))
                .ForMember(dest => dest.ConversationId, src => src.MapFrom(x => x.ConversationId))
                .ForMember(dest => dest.Service, src => src.MapFrom(x => x.Service))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}