using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Common
{
    public class PartyIdMap : Profile
    {
        public PartyIdMap()
        {
            CreateMap<Model.Common.PartyId, Model.Core.PartyId>(MemberList.None)
                .ConstructUsing(src => 
                    string.IsNullOrEmpty(src.Type)
                        ? new Model.Core.PartyId(src.Id) 
                        : new Model.Core.PartyId(src.Id, src.Type));

            CreateMap<Model.Core.PartyId, Model.Common.PartyId>()
                .ForMember(dest => dest.Id, src => src.MapFrom(t => t.Id))
                .ForMember(dest => dest.Type, src => src.MapFrom(t => t.Type ?? string.Empty))
                .AfterMap((corePartyId, commonPartyId) =>
                {
                    if (corePartyId.Type == null && commonPartyId.Type == null)
                        commonPartyId.Type = string.Empty;
                });
        }
    }
}