using System.Collections.Generic;
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
                .ConstructUsing(submit =>
                {
                    var collaboration = new Model.Core.CollaborationInfo(
                        agreement: SubmitMessageAgreementResolver.ResolveAgreementReference(submit),
                        service: SubmitServiceResolver.ResolveService(submit),
                        action: SubmitActionResolver.ResolveAction(submit),
                        conversationId: SubmitConversationIdResolver.ResolveConverstationId(submit));

                    IEnumerable<Model.Core.PartInfo> parts = submit.HasPayloads
                        ? SubmitPayloadInfoResolver.Default.Resolve(submit)
                        : new Model.Core.PartInfo[0];

                    Model.Core.MessageProperty[] properties = submit.MessageProperties?.Any() == true
                        ? SubmitMessagePropertiesResolver.Default.Resolve(submit)
                        : new Model.Core.MessageProperty[0];

                    return new Model.Core.UserMessage(
                        messageId: submit.MessageInfo?.MessageId ?? IdentifierFactory.Instance.Create(),
                        mpc: SubmitMpcResolver.Default.Resolve(submit),
                        collaboration: collaboration,
                        sender: SubmitSenderResolver.ResolveSender(submit),
                        receiver: SubmitReceiverResolver.ResolveReceiver(submit),
                        partInfos: parts,
                        messageProperties: properties);
                })
                .ForMember(dest => dest.RefToMessageId, src => src.MapFrom(s => s.MessageInfo.RefToMessageId))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}