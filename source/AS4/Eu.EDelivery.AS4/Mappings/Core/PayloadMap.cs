using System.Linq;
using AutoMapper;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Utilities;

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

            CreateMap<Xml.PartInfo, Model.Core.PartInfo>()
                .ForMember(dest => dest.Href, src => src.MapFrom(t => t.href))
                .ForMember(dest => dest.Schemas, src => src.MapFrom(t => t.Schemas ?? new Xml.Schema[] {}))
                .ForMember(dest => dest.Properties, src => src.Ignore())
                .AfterMap(
                    (xmlPartInfo, modelPartInfo) =>
                    {
                        if (xmlPartInfo.PartProperties == null || xmlPartInfo.PartProperties.Length == 0) return;
                        modelPartInfo.Properties = xmlPartInfo.PartProperties
                            .ToDictionary(property => property.name, property => property.Value);

                        if (!modelPartInfo.Href.StartsWith("cid:"))
                            throw ThrowNoExternalPayloadsSupportedException(modelPartInfo);
                    });
        }

        private AS4Exception ThrowNoExternalPayloadsSupportedException(Model.Core.PartInfo modelPartInfo)
        {
            return new AS4ExceptionBuilder()
                .WithDescription($"AS4Message only support embedded Payloads and: '{modelPartInfo.Href}' was given")
                .WithErrorCode(ErrorCode.Ebms0011)
                .WithMessageIds(IdGenerator.Generate())
                .WithExceptionType(ExceptionType.ExternalPayloadError)
                .Build();
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