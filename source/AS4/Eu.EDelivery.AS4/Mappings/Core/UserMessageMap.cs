using System.Linq;
using AutoMapper;
using Eu.EDelivery.AS4.Singletons;

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
                        if (!modelUserMessage.MessageProperties.Any())
                        {
                            xmlUserMessage.MessageProperties = null;
                        }

                        if (!modelUserMessage.PayloadInfo.Any())
                        {
                            xmlUserMessage.PayloadInfo = null;
                        }
                    })
                .ForAllOtherMembers(x => x.Ignore());
        }

        private void MapXmlToUserMessage()
        {
            CreateMap<Xml.UserMessage, Model.Core.UserMessage>()
                .ForMember(dest => dest.Mpc, src => src.MapFrom(t => t.mpc))
                .ForMember(dest => dest.MessageId, src => src.MapFrom(t => t.MessageInfo.MessageId))
                .ForMember(dest => dest.RefToMessageId, src => src.MapFrom(t => t.MessageInfo.RefToMessageId))
                .ForMember(dest => dest.Timestamp, src => src.MapFrom(t => t.MessageInfo.Timestamp))
                .ForMember(dest => dest.CollaborationInfo, src => src.MapFrom(t => t.CollaborationInfo))
                .ForMember(dest => dest.MessageProperties,
                           src => src.MapFrom(t => t.MessageProperties ?? new Xml.Property[] { }))
                .ForMember(dest => dest.IsTest, src => src.Ignore())
                .ForMember(dest => dest.IsDuplicate, src => src.Ignore())
                .AfterMap((xml, model) =>
                {
                    if (xml.PayloadInfo != null && xml.PayloadInfo.Any())
                    {
                        foreach (Xml.PartInfo p in
                            xml.PayloadInfo.Where(p => !string.IsNullOrEmpty(p?.href)))
                        {
                            var mapped = AS4Mapper.Map<Model.Core.PartInfo>(p);
                            if (mapped != null)
                            {
                                model.AddPartInfo(mapped);
                            }
                        }
                    }

                    var xmlFrom = xml.PartyInfo?.From;
                    if (xmlFrom != null)
                    {
                        model.Sender = AS4Mapper.Map<Model.Core.Party>(xmlFrom);
                    }

                    var xmlTo = xml.PartyInfo?.To;
                    if (xmlTo != null)
                    {
                        model.Receiver = AS4Mapper.Map<Model.Core.Party>(xmlTo);
                    }

                    Model.Core.CollaborationInfo modelInfo = model.CollaborationInfo;
                    Xml.CollaborationInfo xmlInfo = xml.CollaborationInfo;
                    if (xmlInfo != null)
                    {
                        MapAgreementReference(modelInfo, xmlInfo);

                        model.CollaborationInfo.ConversationId = xml.CollaborationInfo.ConversationId;
                        model.MessageId = xml.MessageInfo.MessageId;
                        model.RefToMessageId = xml.MessageInfo.RefToMessageId;
                    }
                }).ForAllOtherMembers(x => x.Ignore());
        }

        private static void MapAgreementReference(Model.Core.CollaborationInfo modelInfo, Xml.CollaborationInfo xmlInfo)
        {
            if (IsAgreementReferenceEmpty(modelInfo))
            {
                modelInfo.AgreementReference = modelInfo.AgreementReference ?? new Model.Core.AgreementReference();
                modelInfo.AgreementReference.Value = xmlInfo.AgreementRef?.Value;
                modelInfo.AgreementReference.PModeId = xmlInfo.AgreementRef?.pmode;
                modelInfo.AgreementReference.Type = xmlInfo.AgreementRef?.type;
            }
        }

        private static bool IsAgreementReferenceEmpty(Model.Core.CollaborationInfo modelCollaboration)
        {
            return modelCollaboration != null && string.IsNullOrEmpty(modelCollaboration.AgreementReference?.Value);
        }

        private void MapUserMessageToRoutingInputUserMessage()
        {
            CreateMap<Model.Core.UserMessage, Xml.RoutingInputUserMessage>()
                .ForMember(dest => dest.MessageInfo, src => src.MapFrom(t => t))
                .ForMember(dest => dest.CollaborationInfo, src => src.MapFrom(t => t.CollaborationInfo))
                .ForMember(dest => dest.mpc, src => src.MapFrom(t => t.Mpc))
                .ForMember(dest => dest.PayloadInfo, src => src.MapFrom(t => t.PayloadInfo))
                .ForMember(dest => dest.MessageProperties, src => src.MapFrom(t => t.MessageProperties))
                .AfterMap(
                    (modelUserMessage, xmlUserMessage) =>
                    {
                        if (!modelUserMessage.MessageProperties.Any())
                        {
                            xmlUserMessage.MessageProperties = null;
                        }

                        if (!modelUserMessage.PayloadInfo.Any())
                        {
                            xmlUserMessage.PayloadInfo = null;
                        }

                        xmlUserMessage.PartyInfo = new Xml.PartyInfo
                        {
                            From = AS4Mapper.Map<Xml.From>(modelUserMessage.Receiver),
                            To = AS4Mapper.Map<Xml.To>(modelUserMessage.Sender)
                        };

                        AssignAction(xmlUserMessage);
                        AssignMpc(xmlUserMessage);
                    })
                .ForAllOtherMembers(x => x.Ignore());
        }

        private static void AssignAction(Xml.RoutingInputUserMessage xmlUserMessage)
        {
            if (xmlUserMessage.CollaborationInfo?.Action != null)
            {
                xmlUserMessage.CollaborationInfo.Action = $"{xmlUserMessage.CollaborationInfo.Action}.response";
            }
        }

        private static void AssignMpc(Xml.RoutingInputUserMessage xmlUserMessage)
        {
            if (string.IsNullOrEmpty(xmlUserMessage.mpc))
            {
                xmlUserMessage.mpc = Constants.Namespaces.EbmsDefaultMpc;
            }
        }
    }
}