using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Common
{
    public class AgreementReferenceMap : Profile
    {
        public AgreementReferenceMap()
        {
            CreateMap<Model.Common.Agreement, Model.Core.AgreementReference>()
                .ForMember(dest => dest.Type, src => src.MapFrom(s => s.RefType))
                .ForMember(dest => dest.Name, src => src.MapFrom(s => s.Value));

            CreateMap<Model.Core.AgreementReference, Model.Common.Agreement>()
                .ForMember(dest => dest.RefType, src => src.MapFrom(x => x.Type))
                .ForMember(dest => dest.Value, src => src.MapFrom(x => x.Name));
        }
    }
}