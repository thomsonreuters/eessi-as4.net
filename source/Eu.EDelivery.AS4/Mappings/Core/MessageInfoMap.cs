using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class MessageInfoMap : Profile
    {
        public MessageInfoMap()
        {
            CreateMap<Model.Core.MessageUnit, Xml.MessageInfo>()
                .ForMember(dest => dest.MessageId, src => src.MapFrom(t => t.MessageId))
                .ForMember(dest => dest.RefToMessageId, src => src.MapFrom(t => t.RefToMessageId))
                .ForMember(dest => dest.Timestamp, src => src.MapFrom(t => t.Timestamp.UtcDateTime));

            CreateMap<Xml.MessageInfo, Model.Core.MessageUnit>()
                .ForMember(dest => dest.MessageId, src => src.MapFrom(t => t.MessageId))
                .ForMember(dest => dest.RefToMessageId, src => src.MapFrom(t => t.RefToMessageId))
                .ForMember(dest => dest.Timestamp, src => src.MapFrom(t => t.Timestamp));
        }
    }
}