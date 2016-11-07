using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Common
{
    public class PartyMap : Profile
    {
        public PartyMap()
        {
            CreateMap<Model.Common.Party, Model.Core.Party>()
                .ForMember(dest => dest.Role, src => src.MapFrom(t => t.Role))
                .ForMember(dest => dest.PartyIds, src => src.MapFrom(t => t.PartyIds));

            CreateMap<Model.Core.Party, Model.Common.Party>()
                .ForMember(x => x.Role, src => src.MapFrom(t => t.Role))
                .ForMember(dest => dest.PartyIds, src => src.MapFrom(t => t.PartyIds));
        }
    }
}