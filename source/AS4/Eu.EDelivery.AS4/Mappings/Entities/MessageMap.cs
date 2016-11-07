using System.Linq;
using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings
{
    /// <summary>
    /// Map from AS4 Messages to Data Store Messages
    /// </summary>
    public class MessageMap : Profile
    {
        public MessageMap()
        {
            CreateMap<Model.Core.AS4Message, Entities.InMessage>()
                .ForMember(
                    dest => dest.EbmsMessageId,
                    src => src.MapFrom(a => a.SignalMessages.Cast<Model.Core.MessageUnit>().Concat(a.UserMessages).First().MessageId))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Model.Core.AS4Message, Entities.OutMessage>()
                .ForMember(
                    dest => dest.EbmsMessageId,
                    src => src.MapFrom(a => a.SignalMessages.Cast<Model.Core.MessageUnit>().Concat(a.UserMessages).First().MessageId))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}