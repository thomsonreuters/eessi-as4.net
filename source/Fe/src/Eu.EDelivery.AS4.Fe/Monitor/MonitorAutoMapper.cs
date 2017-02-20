using AutoMapper;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public class MonitorAutoMapper : Profile
    {
        public MonitorAutoMapper()
        {
            CreateMap<InMessage, Message>()
                .ForMember(x => x.Status, x => x.MapFrom(y => y.InStatusString))
                .ForMember(x => x.ExceptionType, x => x.MapFrom(y => y.ExceptionTypeString))
                .ForMember(x => x.EbmsMessageType, x => x.MapFrom(y => y.EbmsMessageTypeString))
                .ForMember(x => x.Operation, x => x.MapFrom(y => y.OperationString))
                .ForMember(x => x.ContentType, x => x.MapFrom(y => y.SimplifyContentType()))
                .ForMember(x => x.EbmsRefToMessageId, x => x.MapFrom(y => y.EbmsRefToMessageId));
            CreateMap<OutMessage, Message>()
                .ForMember(x => x.Status, x => x.MapFrom(y => y.OutOutStatusString))
                .ForMember(x => x.ExceptionType, x => x.MapFrom(y => y.ExceptionTypeString))
                .ForMember(x => x.EbmsMessageType, x => x.MapFrom(y => y.EbmsMessageTypeString))
                .ForMember(x => x.Operation, x => x.MapFrom(y => y.OperationString))
                .ForMember(x => x.ContentType, x => x.MapFrom(y => y.SimplifyContentType()));
            CreateMap<InException, ExceptionMessage>()
                .ForMember(x => x.Operation, x => x.MapFrom(y => y.OperationString));
            CreateMap<OutException, ExceptionMessage>()
                .ForMember(x => x.Operation, x => x.MapFrom(y => y.OperationString));
            CreateMap<InMessageJoined, Message>()
                .ProjectUsing(x => Mapper.Map(x.Message, new Message
                {
                    HasExceptions = x.HasExceptions
                }));
            CreateMap<OutMessageJoined, Message>()
                .ProjectUsing(x => Mapper.Map(x.Message, new Message
                {
                    HasExceptions = x.HasExceptions
                }));
        }
    }
}