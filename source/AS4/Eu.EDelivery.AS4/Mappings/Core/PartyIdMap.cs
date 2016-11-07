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

            CreateMap<Xml.PartyId, Model.Core.PartyId>()
                .ForMember(dest => dest.Id, src => src.MapFrom(t => t.Value))
                .ForMember(dest => dest.Type, src => src.MapFrom(t => t.type)).AfterMap(TestPartyId);

            CreateMap<Xml.PartyId[], Model.Core.PartyId>()
                .ForAllOtherMembers(x => x.Ignore());
        }

        private void TestPartyId(Xml.PartyId xmlPartyId, Model.Core.PartyId modelPartyId)
        {
            modelPartyId.Id = xmlPartyId.Value;
            modelPartyId.Type = xmlPartyId.type;
        }
    }
}