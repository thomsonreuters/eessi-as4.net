using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Common
{
    public class ServiceMap : Profile
    {
        public ServiceMap()
        {
            CreateMap<Model.Common.Service, Model.Core.Service>(MemberList.None)
                .ConstructUsing(src => 
                    src.Type == null
                        ? new Model.Core.Service(src.Value)
                        : new Model.Core.Service(src.Value, src.Type));

            CreateMap<Model.Core.Service, Model.Common.Service>(MemberList.None)
                .ConstructUsing(src => 
                    new Model.Common.Service
                    {
                        Value = src.Value,
                        Type = src.Type.GetOrElse(() => null)
                    });
        }
    }
}