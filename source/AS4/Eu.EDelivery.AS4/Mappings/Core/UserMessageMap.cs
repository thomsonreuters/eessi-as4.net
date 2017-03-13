using AutoMapper;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Xml;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class UserMessageMap : Profile
    {
        public UserMessageMap()
        {
            MapUserMessageToXml();
            MapXmlToUserMessage();
            MapUserMessageToRoutingInputUserMessage();
        }

        private void MapUserMessageToXml()
        {
            CreateMap<Model.Core.UserMessage, Xml.UserMessage>()
                .ForMember(dest => dest.MessageInfo, src => src.MapFrom(t => t))
                .ForMember(dest => dest.PartyInfo, src => src.MapFrom(t => t))
                .ForMember(dest => dest.CollaborationInfo, src => src.MapFrom(t => t.CollaborationInfo))
                .ForMember(dest => dest.mpc, src => src.MapFrom(t => t.Mpc))
                .ForMember(dest => dest.PayloadInfo, src => src.MapFrom(t => t.PayloadInfo))
                .ForMember(dest => dest.MessageProperties, src => src.MapFrom(t => t.MessageProperties))
                .AfterMap(
                    (modelUserMessage, xmlUserMessage) =>
                    {
                        if (modelUserMessage.MessageProperties?.Count == 0)
                            xmlUserMessage.MessageProperties = null;

                        if (modelUserMessage.PayloadInfo?.Count == 0)
                            xmlUserMessage.PayloadInfo = null;
                    })
                .ForAllOtherMembers(x => x.Ignore());
        }

        private void MapXmlToUserMessage()
        {
            CreateMap<Xml.UserMessage, Model.Core.UserMessage>()
                .ForMember(dest => dest.MessageId, src => src.MapFrom(t => t.MessageInfo.MessageId))
                .ForMember(dest => dest.RefToMessageId, src => src.MapFrom(t => t.MessageInfo.RefToMessageId))
                .ForMember(dest => dest.Timestamp, src => src.MapFrom(t => t.MessageInfo.Timestamp))
                .ForMember(dest => dest.Sender, src => src.MapFrom(t => t.PartyInfo.From))
                .ForMember(dest => dest.Receiver, src => src.MapFrom(t => t.PartyInfo.To))
                .ForMember(dest => dest.CollaborationInfo, src => src.MapFrom(t => t.CollaborationInfo))
                .ForMember(dest => dest.MessageProperties,
                    src => src.MapFrom(t => t.MessageProperties ?? new Xml.Property[] {}))
                .ForMember(dest => dest.PayloadInfo, src => src.MapFrom(t => t.PayloadInfo ?? new Xml.PartInfo[] {}))
                .ForMember(dest => dest.IsTest, src => src.Ignore())
                .ForMember(dest => dest.IsDuplicate, src => src.Ignore())
                .AfterMap((xmlUserMessage, modelUserMessage) =>
                {
                    Model.Core.CollaborationInfo modelInfo = modelUserMessage.CollaborationInfo;
                    Xml.CollaborationInfo xmlInfo = xmlUserMessage.CollaborationInfo;
                    if (xmlInfo == null) return;
                    MapAgreementReference(modelInfo, xmlInfo);

                    modelUserMessage.CollaborationInfo.ConversationId = xmlUserMessage.CollaborationInfo.ConversationId;
                    modelUserMessage.MessageId = xmlUserMessage.MessageInfo.MessageId;
                    modelUserMessage.RefToMessageId = xmlUserMessage.MessageInfo.RefToMessageId;
                });
        }

        private static void MapAgreementReference(Model.Core.CollaborationInfo modelInfo, Xml.CollaborationInfo xmlInfo)
        {
            if (!IsAgreementReferenceEmpty(modelInfo)) return;

            modelInfo.AgreementReference = modelInfo.AgreementReference ?? new Model.Core.AgreementReference();
            modelInfo.AgreementReference.Value = xmlInfo.AgreementRef?.Value;
            modelInfo.AgreementReference.PModeId = xmlInfo.AgreementRef?.pmode;
            modelInfo.AgreementReference.Type = xmlInfo.AgreementRef?.type;
        }

        private static bool IsAgreementReferenceEmpty(Model.Core.CollaborationInfo modelCollaboration)
        {
            return modelCollaboration != null && string.IsNullOrEmpty(modelCollaboration.AgreementReference?.Value);
        }

        private void MapUserMessageToRoutingInputUserMessage()
        {
            CreateMap<Model.Core.UserMessage, Xml.RoutingInputUserMessage>()
                .ForMember(dest => dest.MessageInfo, src => src.MapFrom(t => t))
                .ForMember(dest => dest.PartyInfo, src => src.MapFrom(t => t))
                .ForMember(dest => dest.CollaborationInfo, src => src.MapFrom(t => t.CollaborationInfo))
                .ForMember(dest => dest.mpc, src => src.MapFrom(t => t.Mpc))
                .ForMember(dest => dest.PayloadInfo, src => src.MapFrom(t => t.PayloadInfo))
                .ForMember(dest => dest.MessageProperties, src => src.MapFrom(t => t.MessageProperties))
                .AfterMap(
                    (modelUserMessage, xmlUserMessage) =>
                    {
                        if (modelUserMessage.MessageProperties?.Count == 0)
                            xmlUserMessage.MessageProperties = null;

                        if (modelUserMessage.PayloadInfo?.Count == 0)
                            xmlUserMessage.PayloadInfo = null;

                        xmlUserMessage.PartyInfo.From = AS4Mapper.Map<Xml.From>(modelUserMessage.Receiver);
                        xmlUserMessage.PartyInfo.To = AS4Mapper.Map<Xml.To>(modelUserMessage.Sender);

                        AssignAction(xmlUserMessage);
                        AssignMpc(xmlUserMessage);
                    })
                .ForAllOtherMembers(x => x.Ignore());
        }

        private static void AssignAction(RoutingInputUserMessage xmlUserMessage)
        {
            if (xmlUserMessage.CollaborationInfo?.Action != null)
                xmlUserMessage.CollaborationInfo.Action = $"{xmlUserMessage.CollaborationInfo.Action}.response";
        }

        private static void AssignMpc(RoutingInputUserMessage xmlUserMessage)
        {
            if (string.IsNullOrEmpty(xmlUserMessage.mpc))
                xmlUserMessage.mpc = Constants.Namespaces.EbmsDefaultMpc;
        }
    }
}