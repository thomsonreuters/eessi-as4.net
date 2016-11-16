using AutoMapper;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Utilities;

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

                .ForMember(dest => dest.Sender, src => src.ResolveUsing(new SubmitSenderPartyResolver().Resolve))
                .ForMember(dest => dest.Receiver, src => src.ResolveUsing(new SubmitReceiverResolver().Resolve))
                .ForMember(dest => dest.MessageProperties, src => src.ResolveUsing(new SubmitMessagePropertiesResolver().Resolve))
                .ForMember(dest => dest.PayloadInfo, src => src.ResolveUsing(new SubmitPayloadInfoResolver().Resolve))

                .ForMember(dest => dest.CollaborationInfo, src => src.MapFrom(s => s.Collaboration))
                .ForMember(dest => dest.IsTest, src => src.Ignore())
                .ForMember(dest => dest.IsDuplicate, src => src.Ignore())
                .ForMember(dest => dest.Mpc, src => src.Ignore())
                .AfterMap(
                    (submitMessage, userMessage) =>
                    {
                        userMessage.MessageId = submitMessage.MessageInfo?.MessageId ?? IdentifierFactory.Instance.Create();

                        new SubmitMessageAgreementMapper().Map(submitMessage, userMessage);

                        userMessage.Mpc = new SubmitMpcResolver().Resolve(submitMessage);
                        userMessage.CollaborationInfo.Service = new SubmitServiceResolver().Resolve(submitMessage);
                        userMessage.CollaborationInfo.Action = new SubmitActionResolver().Resolve(submitMessage);
                        userMessage.CollaborationInfo.ConversationId = new SubmitConversationIdResolver().Resolve(submitMessage);
                    });
        }
    }
}