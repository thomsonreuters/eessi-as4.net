using AutoMapper;
using Eu.EDelivery.AS4.Fe.Pmodes.Model;

namespace Eu.EDelivery.AS4.Fe.Pmodes
{
    public class PmodeAutoMapper : Profile
    {
        public PmodeAutoMapper()
        {
            CreateMap<SendingBasePmode, SendingBasePmode>();
            CreateMap<ReceivingBasePmode, ReceivingBasePmode>();
        }
    }
}