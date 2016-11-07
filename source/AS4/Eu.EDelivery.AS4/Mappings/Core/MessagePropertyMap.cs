using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    /// <summary>
    /// Mapping Message Property (xml > AS4 Model)
    /// </summary>
    public class MessagePropertyMap : Profile
    {
        public MessagePropertyMap()
        {
            CreateMap<Xml.Property, Model.Core.MessageProperty>()
                .ForMember(dest => dest.Name, src => src.MapFrom(t => t.name))
                .ForMember(dest => dest.Type, src => src.MapFrom(t => t.Type))
                .ForMember(dest => dest.Value, src => src.MapFrom(t => t.Value));

            CreateMap<Model.Core.MessageProperty, Xml.Property>()
                .ForMember(dest => dest.name, src => src.MapFrom(t => t.Name))
                .ForMember(dest => dest.Type, src => src.MapFrom(t => t.Type))
                .ForMember(dest => dest.Value, src => src.MapFrom(t => t.Value));
        }
    }
}