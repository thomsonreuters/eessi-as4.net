using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Common
{
    /// <summary>
    /// AutoMapper Profile for Schema (ServiceHandler) > Schema (AS4)
    /// </summary>
    public class SchemaMap : Profile
    {
        public SchemaMap()
        {
            CreateMap<Model.Common.Schema, Model.Core.Schema>()
                .ForMember(dest => dest.Namespace, src => src.MapFrom(s => s.Namespace))
                .ForMember(dest => dest.Location, src => src.MapFrom(s => s.Location))
                .ForMember(dest => dest.Version, src => src.MapFrom(s => s.Version));
        }
    }
}