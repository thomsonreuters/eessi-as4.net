using AutoMapper;
using Eu.EDelivery.AS4.Factories;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    public class SubmitMessageMap : Profile
    {
        public SubmitMessageMap()
        {
            CreateMap<Model.Submit.SubmitMessage, Model.Core.UserMessage>()

                // 1. SubmitMessage / MessageInfo / MessageId
                // 2. Generated according to Settings / GuidFormat
                .ForMember(dest => dest.MessageId, src => src.Ignore())

                // 1. SubmitMessage / MessageInfo / EbmsRefToMessageId 
                // 2. No EbmsRefToMessageId element
                .ForMember(dest => dest.RefToMessageId, src => src.MapFrom(s => s.MessageInfo.RefToMessageId))

                // 1. Generated. In UTC and in XML schema Date Time format (ISO8601), with the ‘Z’ time zone indicator being optional.
                .ForMember(dest => dest.Timestamp, src => src.Ignore())

                .ForMember(dest => dest.Sender, src => src.ResolveUsing(SubmitSenderPartyResolver.Default.Resolve))
                .ForMember(dest => dest.Receiver, src => src.ResolveUsing(SubmitReceiverResolver.Default.Resolve))
                .ForMember(dest => dest.MessageProperties, src => src.ResolveUsing(SubmitMessagePropertiesResolver.Default.Resolve))

                .ForMember(dest => dest.CollaborationInfo, src => src.MapFrom(s => s.Collaboration))
                .ForMember(dest => dest.IsTest, src => src.Ignore())
                .ForMember(dest => dest.IsDuplicate, src => src.Ignore())
                .ForMember(dest => dest.Mpc, src => src.Ignore())
                .AfterMap(
                    (submitMessage, userMessage) =>
                    {
                        userMessage.MessageId = submitMessage.MessageInfo?.MessageId ?? IdentifierFactory.Instance.Create();

                        new SubmitMessageAgreementMapper().Map(submitMessage, userMessage);

                        userMessage.Mpc = SubmitMpcResolver.Default.Resolve(submitMessage);
                        userMessage.CollaborationInfo.Service = SubmitServiceResolver.Default.Resolve(submitMessage);
                        userMessage.CollaborationInfo.Action =  SubmitActionResolver.Default.Resolve(submitMessage);
                        userMessage.CollaborationInfo.ConversationId = SubmitConversationIdResolver.Default.Resolve(submitMessage);

                        if (submitMessage.HasPayloads)
                        {
                            foreach (Model.Core.PartInfo p in 
                                SubmitPayloadInfoResolver.Default.Resolve(submitMessage))
                            {
                                userMessage.AddPartInfo(p);
                            }
                        }
                    });
        }
    }
}