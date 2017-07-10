using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using AutoMapper;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Singletons;
using NonRepudiationInformation = Eu.EDelivery.AS4.Xml.NonRepudiationInformation;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class SignalMessageMap : Profile
    {
        public SignalMessageMap()
        {
            CreateMap<SignalMessage, Xml.SignalMessage>().ForAllOtherMembers(x => x.Ignore());

            CreateMap<Xml.SignalMessage, Receipt>()
                .ForMember(dest => dest.RefToMessageId, src => src.MapFrom(t => t.MessageInfo.RefToMessageId))
                .ForMember(dest => dest.MessageId, src => src.MapFrom(t => t.MessageInfo.MessageId))
                .ForMember(dest => dest.Timestamp, src => src.MapFrom(t => t.MessageInfo.Timestamp))
                .ForMember(dest => dest.UserMessage, src => src.MapFrom(t => t.Receipt.UserMessage))
                .ForMember(dest => dest.IsDuplicate, src => src.Ignore())
                .ForMember(
                    dest => dest.NonRepudiationInformation,
                    src => src.MapFrom(t => t.Receipt.NonRepudiationInformation))
                .AfterMap(
                    (message, receipt) =>
                    {
                        XmlElement firstElement = message.Receipt.Any?.FirstOrDefault();

                        if (firstElement != null
                            && firstElement.LocalName.IndexOf(
                                "NonRepudiationInformation",
                                StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            var serializer = new XmlSerializer(typeof(NonRepudiationInformation));
                            object deserialize = serializer.Deserialize(new XmlNodeReader(firstElement));

                            receipt.NonRepudiationInformation =
                                AS4Mapper.Map<Model.Core.NonRepudiationInformation>(deserialize);
                        }
                    }).ForAllOtherMembers(t => t.Ignore());

            CreateMap<Xml.SignalMessage, PullRequest>()
                .ConstructUsing(source => new PullRequest(source.PullRequest.mpc))                
                .ForAllOtherMembers(t => t.Ignore());
        }
    }
}