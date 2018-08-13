using AutoMapper;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class SignalMessageMap : Profile
    {
        public SignalMessageMap()
        {
            CreateMap<SignalMessage, Xml.SignalMessage>()
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}