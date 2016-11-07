using AutoMapper;

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

            CreateMap<Xml.Receipt, Model.Core.Receipt>()
                .ForMember(dest => dest.UserMessage, src => src.MapFrom(t => t.UserMessage))
                .ForMember(dest => dest.NonRepudiationInformation, src => src.MapFrom(t => t.NonRepudiationInformation))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}
