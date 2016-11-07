using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class ServiceMap : Profile
    {
        public ServiceMap()
        {
            CreateMap<Model.Core.Service, Xml.Service>()
                .ForMember(dest => dest.type, src => src.MapFrom(t => t.Type))
                .ForMember(dest => dest.Value, src => src.MapFrom(t => t.Name));

            CreateMap<Xml.Service, Model.Core.Service>()
                .ForMember(dest => dest.Type, src => src.MapFrom(t => t.type))
                .ForMember(dest => dest.Name, src => src.MapFrom(t => t.Value));
        }
    }
}