using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    internal class AgreementRefMap : Profile
    {
        public AgreementRefMap()
        {
            CreateMap<Model.Core.AgreementReference, Xml.AgreementRef>()
                .ForMember(dest => dest.Value, src => src.MapFrom(t => t.Name))
                .ForMember(dest => dest.pmode, src => src.MapFrom(t => t.PModeId))
                .ForMember(dest => dest.type, src => src.MapFrom(t => t.Type));

            CreateMap<Xml.AgreementRef, Model.Core.AgreementReference>()
                .ForMember(dest => dest.Name, src => src.MapFrom(t => t.Value))
                .ForMember(dest => dest.PModeId, src => src.MapFrom(t => t.pmode))
                .ForMember(dest => dest.Type, src => src.MapFrom(t => t.type));
        }
    }
}