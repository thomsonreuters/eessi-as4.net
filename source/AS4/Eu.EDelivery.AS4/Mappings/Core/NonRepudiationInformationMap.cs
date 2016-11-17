using System;
using System.Linq;
using System.Text;
using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class NonRepudiationInformationMap : Profile
    {
        public NonRepudiationInformationMap()
        {
            MapModelToXml();
            MapXmlToModel();
        }

        private void MapModelToXml()
        {
            CreateMap<Model.Core.NonRepudiationInformation, Xml.NonRepudiationInformation>()
                .ForMember(dest => dest.MessagePartNRInformation, src => src.MapFrom(t => t.MessagePartNRInformation));

            CreateMap<Model.Core.MessagePartNRInformation, Xml.MessagePartNRInformation>()
                .ForMember(dest => (Xml.ReferenceType) dest.Item, src => src.MapFrom(t => t.Reference))
                .AfterMap((modelInfo, xmlInfo) =>
                {
                    Model.Core.Reference modelReference = modelInfo.Reference;
                    xmlInfo.Item = new Xml.ReferenceType
                    {
                        URI = modelReference.URI,
                        DigestMethod = new Xml.DigestMethodType {Algorithm = modelReference.DigestMethod.Algorithm},
                        DigestValue = Encoding.UTF8.GetBytes(modelInfo.Reference.DigestValue),
                        Transforms = modelReference.Transforms
                            .Select(t => new Xml.TransformType {Algorithm = t.Algorithm}).ToArray()
                    };
                });

            CreateMap<Model.Core.Reference, Xml.ReferenceType>()
                .ForMember(dest => dest.Transforms, src => src.MapFrom(t => t.Transforms))
                .ForMember(dest => dest.DigestMethod, src => src.MapFrom(t => t.DigestMethod))
                .ForMember(dest => dest.DigestValue, src => src.MapFrom(t => t.DigestValue))
                .ForMember(dest => dest.URI, src => src.MapFrom(t => t.URI))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Model.Core.ReferenceTransform, Xml.TransformType>()
                .ForMember(dest => dest.Algorithm, src => src.MapFrom(t => t.Algorithm))
                .ForAllMembers(x => x.Ignore());

            CreateMap<Model.Core.ReferenceDigestMethod, Xml.DigestMethodType>()
                .ForMember(dest => dest.Algorithm, src => src.MapFrom(t => t.Algorithm))
                .ForAllOtherMembers(x => x.Ignore());
        }

        private void MapXmlToModel()
        {
            CreateMap<Xml.NonRepudiationInformation, Model.Core.NonRepudiationInformation>()
                .ForMember(dest => dest.MessagePartNRInformation, src => src.MapFrom(t => t.MessagePartNRInformation));

            CreateMap<Xml.MessagePartNRInformation, Model.Core.MessagePartNRInformation>()
                .ForMember(dest => dest.Reference, src => src.MapFrom(t => t.Item));

            CreateMap<Xml.ReferenceType, Model.Core.Reference>()
                .ForMember(dest => dest.Transforms, src => src.MapFrom(t => t.Transforms))
                .ForMember(dest => dest.DigestMethod, src => src.MapFrom(t => t.DigestMethod))
                .ForMember(dest => dest.DigestValue, src => src.MapFrom(t => Convert.ToBase64String(t.DigestValue)))
                .ForMember(dest => dest.URI, src => src.MapFrom(t => t.URI))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Xml.TransformType, Model.Core.ReferenceTransform>()
                .ForMember(dest => dest.Algorithm, src => src.MapFrom(t => t.Algorithm))
                .ForAllMembers(x => x.Ignore());

            CreateMap<Xml.DigestMethodType, Model.Core.ReferenceDigestMethod>()
                .ForMember(dest => dest.Algorithm, src => src.MapFrom(t => t.Algorithm))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}