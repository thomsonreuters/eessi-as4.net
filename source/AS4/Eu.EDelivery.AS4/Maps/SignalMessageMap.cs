using AutoMapper;
using Eu.EDelivery.AS4.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Mappings
{
    public class SignalMessageMap : Profile
    {
        public SignalMessageMap()
        {
            //CreateMap<SignalMessage, Xml.>
            CreateMap<SignalMessage, Xml.SignalMessage>()
                .ForMember(x => x.MessageInfo, x => x.MapFrom(t => t))
                //.BeforeMap((src, dst) =>
                //{
                //    dst.MessageInfo = Mapper.Map<SignalMessage, Xml.MessageInfo>(src);
                //})
                .ForAllOtherMembers(x => x.Ignore())
                ;
        }
    }
}
