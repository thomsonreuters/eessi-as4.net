using System.Linq;
using AutoMapper;
using Eu.EDelivery.AS4.Singletons;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class PayloadMap : Profile
    {
        public PayloadMap()
        {
            CreateMap<Model.Core.PartInfo, Xml.PartInfo>()
                .ForMember(dest => dest.href, src => src.MapFrom(t => t.Href))
                .ForMember(dest => dest.Schemas, src => src.MapFrom(t => t.Schemas))
                .ForMember(dest => dest.Description, src => src.Ignore())
                .ForMember(dest => dest.PartProperties, src => src.Ignore())
                .AfterMap(
                    (modelPartInfo, xmlPartInfo) =>
                    {
                        xmlPartInfo.PartProperties = modelPartInfo.Properties
                            .Select(p => new Xml.Property {name = p.Key, Value = p.Value}).ToArray();
                    });

            CreateMap<Xml.PartInfo, Model.Core.PartInfo>(MemberList.None)
                .ConstructUsing(src =>
                {
                    Xml.Property[] props = src.PartProperties ?? new Xml.Property[0];
                    Xml.Schema[] schemas = src.Schemas ?? new Xml.Schema[0];

                    return new Model.Core.PartInfo(
                        src.href,
                        props.ToDictionary(prop => prop.name, p => p.Value),
                        schemas.Select(AS4Mapper.Map<Model.Core.Schema>));
                });
        }
    }

    public class SchemaMap : Profile
    {
        public SchemaMap()
        {
            CreateMap<Model.Core.Schema, Xml.Schema>()
                .ForMember(dest => dest.location, src => src.MapFrom(t => t.Location))
                .ForMember(dest => dest.version, src => src.MapFrom(t => t.Version))
                .ForMember(dest => dest.@namespace, src => src.MapFrom(t => t.Namespace));

            CreateMap<Xml.Schema, Model.Core.Schema>()
                .ForMember(dest => dest.Location, src => src.MapFrom(t => t.location))
                .ForMember(dest => dest.Version, src => src.MapFrom(t => t.version))
                .ForMember(dest => dest.Namespace, src => src.MapFrom(t => t.@namespace));
        }
    }
}