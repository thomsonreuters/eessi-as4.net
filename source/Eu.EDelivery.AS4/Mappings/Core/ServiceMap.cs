using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class ServiceMap : Profile
    {
        public ServiceMap()
        {
            CreateMap<Model.Core.Service, Xml.Service>()
                .ConstructUsing(model => new Xml.Service
                {
                    Value = model.Value,
                    type = model.Type.GetOrElse(() => null)
                }).ForAllOtherMembers(x => x.Ignore());

            CreateMap<Xml.Service, Model.Core.Service>()
                .ConstructUsing(xml => 
                    xml.type == null
                        ? new Model.Core.Service(xml.Value)
                        : new Model.Core.Service(xml.Value, xml.type))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}