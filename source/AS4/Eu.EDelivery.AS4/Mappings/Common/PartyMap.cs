using System.Linq;
using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Common
{
    public class PartyMap : Profile
    {
        public PartyMap()
        {
            CreateMap<Model.Common.Party, Model.Core.Party>(MemberList.None)
                .ConstructUsing(
                    src => new Model.Core.Party(
                        src.Role,
                        src.PartyIds?.Select(
                            id => new Model.Core.PartyId(id.Id, id.Type))));

            CreateMap<Model.Core.Party, Model.Common.Party>()
                .ForMember(x => x.Role, src => src.MapFrom(t => t.Role))
                .ForMember(dest => dest.PartyIds, src => src.MapFrom(t => t.PartyIds));
        }
    }
}