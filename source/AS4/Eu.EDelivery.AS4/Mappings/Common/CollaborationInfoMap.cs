using AutoMapper;
using Eu.EDelivery.AS4.Singletons;

namespace Eu.EDelivery.AS4.Mappings.Common
{
    public class CollaborationInfoMap : Profile
    {
        public CollaborationInfoMap()
        {
            CreateMap<Model.Common.CollaborationInfo, Model.Core.CollaborationInfo>()
                .ConstructUsing(src =>
                {
                    Maybe<Model.Core.AgreementReference> a = (src.AgreementRef?.Value != null)
                        .ThenMaybe(() => AS4Mapper.Map<Model.Core.AgreementReference>(src.AgreementRef));

                    Model.Core.Service s = src.Service != null
                        ? AS4Mapper.Map<Model.Core.Service>(src.Service)
                        : Model.Core.Service.TestService;

                    string action = src.Action ?? Constants.Namespaces.TestAction;
                    string conversationId = src.ConversationId ?? Model.Core.CollaborationInfo.DefaultConversationId;

                    return new Model.Core.CollaborationInfo(a, s, action, conversationId);
                }).ForAllOtherMembers(x => x.Ignore());

            CreateMap<Model.Core.CollaborationInfo, Model.Common.CollaborationInfo>()
                .ForMember(dest => dest.Action, src => src.MapFrom(x => x.Action))
                .ForMember(dest => dest.ConversationId, src => src.MapFrom(x => x.ConversationId))
                .ForMember(dest => dest.Service, src => src.MapFrom(x => x.Service))
                .AfterMap((src, dest) =>
                {
                    src.AgreementReference.Do(
                        a => dest.AgreementRef = AS4Mapper.Map<Model.Common.Agreement>(a));
                })
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}