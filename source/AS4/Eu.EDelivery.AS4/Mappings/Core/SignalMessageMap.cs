using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using AutoMapper;
using Eu.EDelivery.AS4.Singletons;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class SignalMessageMap : Profile
    {
        public SignalMessageMap()
        {
            CreateMap<Model.Core.SignalMessage, Xml.SignalMessage>()
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Xml.SignalMessage, Model.Core.Receipt>()
                .ForMember(dest => dest.RefToMessageId, src => src.MapFrom(t => t.MessageInfo.RefToMessageId))
                .ForMember(dest => dest.MessageId, src => src.MapFrom(t => t.MessageInfo.MessageId))
                .ForMember(dest => dest.Timestamp, src => src.MapFrom(t => t.MessageInfo.Timestamp))
                .ForMember(dest => dest.UserMessage, src => src.MapFrom(t => t.Receipt.UserMessage))
                .ForMember(dest => dest.IsDuplicated, src => src.Ignore())
                .ForMember(dest => dest.NonRepudiationInformation, src => src.MapFrom(t => t.Receipt.NonRepudiationInformation))
                .AfterMap(((message, receipt) =>
                {
                    if (message.Receipt.Any?.FirstOrDefault()?.LocalName.Contains("NonRepudiationInformation") == true)
                    {
                        var serializer = new XmlSerializer(typeof(Xml.NonRepudiationInformation));
                        object deserialize = serializer.Deserialize(new XmlNodeReader(message.Receipt.Any.FirstOrDefault()));
                        receipt.NonRepudiationInformation = AS4Mapper.Map<Model.Core.NonRepudiationInformation>(deserialize);
                    }
                }));
        }
    }
}