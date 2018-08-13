using AutoMapper;
using Eu.EDelivery.AS4.Singletons;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class CollaborationInfoMap : Profile
    {
        public CollaborationInfoMap()
        {
            CreateMap<Model.Core.CollaborationInfo, Xml.CollaborationInfo>()
                .ForMember(dest => dest.Action, src => src.MapFrom(t => t.Action))
                .ForMember(dest => dest.ConversationId, src => src.MapFrom(t => t.ConversationId))
                .ForMember(dest => dest.Service, src => src.MapFrom(t => t.Service))
                .AfterMap((modelInfo, xmlInfo) =>
                {
                    modelInfo?.AgreementReference?.Do(
                        a => xmlInfo.AgreementRef = AS4Mapper.Map<Xml.AgreementRef>(a));
                }).ForAllOtherMembers(x => x.Ignore());

            CreateMap<Xml.CollaborationInfo, Model.Core.CollaborationInfo>()
                .ConstructUsing(xml =>
                {
                    Maybe<Model.Core.AgreementReference> a = (xml.AgreementRef?.Value != null)
                        .ThenMaybe(() => AS4Mapper.Map<Model.Core.AgreementReference>(xml.AgreementRef));

                    Model.Core.Service s = xml.Service != null 
                        ? AS4Mapper.Map<Model.Core.Service>(xml.Service) 
                        : Model.Core.Service.TestService;

                    string action = xml.Action ?? Constants.Namespaces.TestAction;
                    string conversationId = xml.ConversationId ?? Model.Core.CollaborationInfo.DefaultConversationId;

                    return new Model.Core.CollaborationInfo(a, s, action, conversationId);
                }).ForAllOtherMembers(x => x.Ignore());
        }
    }
}