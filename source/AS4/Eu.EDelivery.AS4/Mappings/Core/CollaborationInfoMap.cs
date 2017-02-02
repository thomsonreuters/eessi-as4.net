using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class CollaborationInfoMap : Profile
    {
        public CollaborationInfoMap()
        {
            CreateMap<Model.Core.CollaborationInfo, Xml.CollaborationInfo>()
                .ForMember(dest => dest.Action, src => src.MapFrom(t => t.Action))
                .ForMember(dest => dest.AgreementRef, src => src.MapFrom(t => t.AgreementReference))
                .ForMember(dest => dest.ConversationId, src => src.MapFrom(t => t.ConversationId))
                .ForMember(dest => dest.Service, src => src.MapFrom(t => t.Service))
                .AfterMap((modelInfo, xmlInfo) =>
                {
                    if (modelInfo?.AgreementReference?.IsEmpty() == true)
                        xmlInfo.AgreementRef = null;
                });

            CreateMap<Xml.CollaborationInfo, Model.Core.CollaborationInfo>()
                .ForMember(dest => dest.Action, src => src.MapFrom(t => t.Action))
                .ForMember(dest => dest.AgreementReference, src => src.MapFrom(t => t.AgreementRef))
                .ForMember(dest => dest.ConversationId, src => src.MapFrom(t => t.ConversationId))
                .ForMember(dest => dest.Service, src => src.MapFrom(t => t.Service))
                .AfterMap((xmlInfo, modelInfo) =>
                {
                    if (modelInfo?.AgreementReference?.IsEmpty() == true)
                        modelInfo.AgreementReference = null;
                });
        }
    }
}