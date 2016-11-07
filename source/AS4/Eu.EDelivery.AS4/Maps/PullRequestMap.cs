using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Mappings
{
    public class PullRequestMap : Profile
    {
        public PullRequestMap()
        {
            CreateMap<Model.PullRequest, Xml.SignalMessage>()
                .IncludeBase<Model.SignalMessage, Xml.SignalMessage>()
                .ForMember(x => x.PullRequest, x => x.MapFrom(t => t))
                //.ForMember(x => x.PullRequest, x => x.MapFrom(t => t))
                .ForAllOtherMembers(x => x.Ignore())
                ;

            CreateMap<Model.PullRequest, Xml.PullRequest>()
                .ForMember(x => x.mpc, x => x.MapFrom(t => t.Mpc))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}
