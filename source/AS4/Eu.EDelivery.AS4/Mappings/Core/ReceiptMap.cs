using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using AutoMapper;
using Eu.EDelivery.AS4.Singletons;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class ReceiptMap : Profile
    {
        public ReceiptMap()
        {
            CreateMap<Model.Core.Receipt, Xml.SignalMessage>()
                .ForMember(dest => dest.Receipt, src => src.MapFrom(t => t))
                .ForMember(dest => dest.MessageInfo, src => src.MapFrom(t => t))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Model.Core.Receipt, Xml.Receipt>()
                .ForMember(dest => dest.UserMessage, src => src.MapFrom(t => t.UserMessage))
                .ForMember(dest => dest.NonRepudiationInformation, src => src.MapFrom(t => t.NonRepudiationInformation))
               .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Xml.SignalMessage, Model.Core.Receipt>()
                .ConstructUsing(xml =>
                {
                    var timestamp = xml.MessageInfo?.Timestamp ?? default(DateTimeOffset);
                    XmlElement firstNrrElement = xml.Receipt.Any?.FirstOrDefault();

                    if (firstNrrElement != null
                        && firstNrrElement.LocalName.IndexOf(
                            "NonRepudiationInformation",
                            StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        var serializer = new XmlSerializer(typeof(Model.Core.NonRepudiationInformation));
                        object deserialize = serializer.Deserialize(new XmlNodeReader(firstNrrElement));
                        var nonRepudiation = AS4Mapper.Map<Model.Core.NonRepudiationInformation>(deserialize);

                        return new Model.Core.Receipt(
                                xml.MessageInfo?.MessageId,
                                xml.MessageInfo?.RefToMessageId,
                                timestamp,
                                nonRepudiation);
                    }

                    if (xml.Receipt.NonRepudiationInformation != null)
                    {
                        return new Model.Core.Receipt(
                                xml.MessageInfo?.MessageId,
                                xml.MessageInfo?.RefToMessageId,
                                timestamp,
                                AS4Mapper.Map<Model.Core.NonRepudiationInformation>(
                                    xml.Receipt.NonRepudiationInformation));
                    }

                    if (xml.Receipt.UserMessage != null)
                    {
                        return new Model.Core.Receipt(
                            xml.MessageInfo?.MessageId,
                            xml.MessageInfo?.RefToMessageId,
                            timestamp,
                            AS4Mapper.Map<Model.Core.UserMessage>(xml.Receipt.UserMessage)); 
                    }

                    return new Model.Core.Receipt(
                        xml.MessageInfo?.MessageId,
                        xml.MessageInfo?.RefToMessageId,
                        timestamp);

                }).ForAllOtherMembers(t => t.Ignore());
        }
    }
}
