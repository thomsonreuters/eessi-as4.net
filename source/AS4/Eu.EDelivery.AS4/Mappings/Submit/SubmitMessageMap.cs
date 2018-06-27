using System.Linq;
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

                .ForMember(dest => dest.IsTest, src => src.Ignore())
                .ForMember(dest => dest.IsDuplicate, src => src.Ignore())
                .ForMember(dest => dest.Mpc, src => src.Ignore())
                .AfterMap(
                    (submit, user) =>
                    {
                        user.MessageId = submit.MessageInfo?.MessageId ?? IdentifierFactory.Instance.Create();

                        user.Mpc = SubmitMpcResolver.Default.Resolve(submit);

                        user.CollaborationInfo = new Model.Core.CollaborationInfo(
                            SubmitMessageAgreementResolver.ResolveAgreementReference(submit, user),
                            SubmitServiceResolver.ResolveService(submit),
                            SubmitActionResolver.ResolveAction(submit),
                            SubmitConversationIdResolver.ResolveConverstationId(submit));

                        if (submit.HasPayloads)
                        {
                            foreach (Model.Core.PartInfo p in 
                                SubmitPayloadInfoResolver.Default.Resolve(submit))
                            {
                                user.AddPartInfo(p);
                            }
                        }

                        if (submit.MessageProperties?.Any() == true)
                        {
                            foreach (Model.Core.MessageProperty p in
                                SubmitMessagePropertiesResolver.Default.Resolve(submit))
                            {
                                user.AddMessageProperty(p);
                            }
                        }
                    }).ForAllOtherMembers(x => x.Ignore());
        }
    }
}