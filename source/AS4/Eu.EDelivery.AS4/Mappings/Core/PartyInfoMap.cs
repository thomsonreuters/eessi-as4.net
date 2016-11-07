using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class PartyInfoMap : Profile
    {
        public PartyInfoMap()
        {
            CreateMap<Model.Core.UserMessage, Xml.PartyInfo>()
                .ForMember(dest => dest.From, src => src.MapFrom(t => t.Sender))
                .ForMember(dest => dest.To, src => src.MapFrom(t => t.Receiver));

            CreateMap<Model.Core.Party, Xml.PartyInfo>()
                .ForMember(dest => dest.From, src => src.MapFrom(t => t))
                .ForMember(dest => dest.To, src => src.MapFrom(t => t))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Model.Core.Party, Xml.From>()
                .ForMember(dest => dest.Role, src => src.MapFrom(t => t.Role))
                .ForMember(dest => dest.PartyId, src => src.MapFrom(t => t.PartyIds))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Xml.PartyInfo, Model.Core.Party>()
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Xml.From, Model.Core.Party>()
                .ForMember(dest => dest.Role, src => src.MapFrom(t => t.Role))
                .ForMember(dest => dest.PartyIds, src => src.MapFrom(t => t.PartyId))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Model.Core.Party, Xml.To>()
                .ForMember(dest => dest.Role, src => src.MapFrom(t => t.Role))
                .ForMember(dest => dest.PartyId, src => src.MapFrom(t => t.PartyIds))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Xml.To, Model.Core.Party>()
                .ForMember(dest => dest.Role, src => src.MapFrom(t => t.Role))
                .ForMember(dest => dest.PartyIds, src => src.MapFrom(t => t.PartyId))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}