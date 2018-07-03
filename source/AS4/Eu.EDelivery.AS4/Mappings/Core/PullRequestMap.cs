using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class PullRequestMap : Profile
    {
        public PullRequestMap()
        {
            CreateMap<Model.Core.PullRequest, Xml.SignalMessage>()
                .ForMember(dest => dest.PullRequest, src => src.MapFrom(t => t))
                .ForMember(dest => dest.MessageInfo, src => src.MapFrom(t => t))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Model.Core.PullRequest, Xml.PullRequest>()
                .ForMember(dest => dest.mpc, src => src.MapFrom(t => t.Mpc))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Xml.PullRequest, Model.Core.PullRequest>()
                .ConstructUsing(xml => new Model.Core.PullRequest(xml.mpc))                
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Xml.SignalMessage, Model.Core.PullRequest>()
                .ConstructUsing(source => new Model.Core.PullRequest(source.PullRequest.mpc))
                .ForAllOtherMembers(t => t.Ignore());
        }
    }
}