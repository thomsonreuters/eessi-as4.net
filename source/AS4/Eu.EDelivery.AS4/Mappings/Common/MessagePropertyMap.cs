using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Common
{
    /// <summary>
    /// Mapping MessageProperty (Submit Model > AS4 Model)
    /// </summary>
    public class MessagePropertyMap : Profile
    {
        public MessagePropertyMap()
        {
            CreateMap<Model.Common.MessageProperty, Model.Core.MessageProperty>()
                .ForMember(dest => dest.Name, src => src.MapFrom(x => x.Name))
                .ForMember(dest => dest.Value, src => src.MapFrom(x => x.Value))
                .ForMember(dest => dest.Type, src => src.MapFrom(x => x.Type));

            CreateMap<Model.Core.MessageProperty, Model.Common.MessageProperty>()
                .ForMember(dest => dest.Name, src => src.MapFrom(x => x.Name))
                .ForMember(dest => dest.Value, src => src.MapFrom(x => x.Value))
                .ForMember(dest => dest.Type, src => src.MapFrom(x => x.Type));
        }
    }
}