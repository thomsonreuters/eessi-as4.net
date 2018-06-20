using AutoMapper;
using Eu.EDelivery.AS4.Factories;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    public class SubmitMessageMap : Profile
    {
        public SubmitMessageMap()
        {
            CreateMap<Model.Submit.SubmitMessage, Model.Core.UserMessage>()
                .ForMember(dest => dest.MessageId, src => src.Ignore())
                .ForMember(dest => dest.RefToMessageId, src => src.MapFrom(s => s.MessageInfo.RefToMessageId))
                .ForMember(dest => dest.Timestamp, src => src.Ignore())
                .ForMember(dest => dest.MessageProperties, src => src.ResolveUsing(SubmitMessagePropertiesResolver.Default.Resolve))
                .ForMember(dest => dest.CollaborationInfo, src => src.MapFrom(s => s.Collaboration))
                .ForMember(dest => dest.IsTest, src => src.Ignore())
                .ForMember(dest => dest.IsDuplicate, src => src.Ignore())
                .ForMember(dest => dest.Mpc, src => src.Ignore())
                .AfterMap(
                    (submit, user) =>
                    {
                        user.MessageId = submit.MessageInfo?.MessageId ?? IdentifierFactory.Instance.Create();

                        user.Sender = SubmitSenderResolver.ResolveSender(submit);
                        user.Receiver = SubmitReceiverResolver.ResolveReceiver(submit);

                        new SubmitMessageAgreementMapper().Map(submit, user);

                        user.Mpc = SubmitMpcResolver.Default.Resolve(submit);
                        user.CollaborationInfo.Service = SubmitServiceResolver.Default.Resolve(submit);
                        user.CollaborationInfo.Action =  SubmitActionResolver.Default.Resolve(submit);
                        user.CollaborationInfo.ConversationId = SubmitConversationIdResolver.Default.Resolve(submit);

                        if (submit.HasPayloads)
                        {
                            foreach (Model.Core.PartInfo p in 
                                SubmitPayloadInfoResolver.Default.Resolve(submit))
                            {
                                user.AddPartInfo(p);
                            }
                        }

                    }).ForAllOtherMembers(x => x.Ignore());
        }
    }
}