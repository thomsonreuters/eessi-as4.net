using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Eu.EDelivery.AS4.Singletons;

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
                .ConstructUsing(model => 
                    new Xml.NonRepudiationInformation
                    {
                        MessagePartNRInformation = model.MessagePartNRIReferences.Select(r => 
                            new Xml.MessagePartNRInformation
                            {
                                Item = new Xml.ReferenceType
                                {
                                    URI = r.URI,
                                    DigestMethod = new Xml.DigestMethodType { Algorithm = r.DigestMethod.Algorithm },
                                    DigestValue = r.DigestValue,
                                    Transforms = r.Transforms
                                                  .Select(t => new Xml.TransformType { Algorithm = t.Algorithm })
                                                  .ToArray()
                                }
                            }).ToArray()
                    })
                .ForAllOtherMembers(x => x.Ignore());

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
                .ConstructUsing(xml =>
                {
                    IEnumerable<Model.Core.Reference> references = 
                        xml.MessagePartNRInformation
                            .Select(p => p.Item)
                            .Where(i => i != null)
                            .Cast<Xml.ReferenceType>()
                            .Select(AS4Mapper.Map<Model.Core.Reference>);

                    return new Model.Core.NonRepudiationInformation(references);
                })
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Xml.ReferenceType, Model.Core.Reference>()
                .ConstructUsing(xml =>
                    new Model.Core.Reference(
                        xml.URI,
                        xml.Transforms?.Select(t => new Model.Core.ReferenceTransform(t.Algorithm)),
                        new Model.Core.ReferenceDigestMethod(xml.DigestMethod?.Algorithm), 
                        xml.DigestValue))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}