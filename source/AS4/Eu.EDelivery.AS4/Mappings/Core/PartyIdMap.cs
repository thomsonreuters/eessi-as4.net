using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class PartyIdMap : Profile
    {
        public PartyIdMap()
        {
            CreateMap<Model.Core.PartyId, Xml.PartyId>()
                .ForMember(dest => dest.Value, src => src.MapFrom(t => t.Id))
                .ForMember(dest => dest.type, src => src.MapFrom(t => t.Type));

            CreateMap<Xml.PartyId, Model.Core.PartyId>(MemberList.None)
                .ConstructUsing(xml => new Model.Core.PartyId(xml.Value, xml.type));

            CreateMap<Xml.PartyId[], Model.Core.PartyId>()
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}