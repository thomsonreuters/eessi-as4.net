using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Common
{
    public class ServiceMap : Profile
    {
        public ServiceMap()
        {
            CreateMap<Model.Common.Service, Model.Core.Service>()
                .ForMember(dest => dest.Name, src => src.MapFrom(s => s.Value));

            CreateMap<Model.Core.Service, Model.Common.Service>()
                .ForMember(dest => dest.Value, src => src.MapFrom(x => x.Name))
                .ForMember(dest => dest.Type, src => src.MapFrom(x => x.Type))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}