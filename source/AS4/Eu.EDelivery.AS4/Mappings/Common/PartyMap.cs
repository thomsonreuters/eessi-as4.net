using System.Linq;
using AutoMapper;
using Eu.EDelivery.AS4.Singletons;

namespace Eu.EDelivery.AS4.Mappings.Common
{
    public class PartyMap : Profile
    {
        public PartyMap()
        {
            CreateMap<Model.Common.Party, Model.Core.Party>()
                .ConstructUsing(src => 
                        new Model.Core.Party(
                        src.Role,
                        src.PartyIds?.Select(AS4Mapper.Map<Model.Core.PartyId>)))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Model.Core.Party, Model.Common.Party>()
                .ForMember(x => x.Role, src => src.MapFrom(t => t.Role))
                .ForMember(dest => dest.PartyIds, src => src.MapFrom(t => t.PartyIds));
        }
    }
}