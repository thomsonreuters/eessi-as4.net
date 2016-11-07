using AutoMapper;
using Eu.EDelivery.AS4.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Mappings
{
    public class MessageInfoMap : Profile
    {
        public MessageInfoMap()
        {
            CreateMap<MessageUnit, Xml.MessageInfo>()
                .ForMember(x => x.MessageId, x => x.MapFrom(t => t.MessageId))
                .ForMember(x => x.RefToMessageId, x => x.MapFrom(t => t.RefToMessageId))
                .ForMember(x => x.Timestamp, x => x.MapFrom(t => t.Timestamp.UtcDateTime))
                ;
        }
    }
}
