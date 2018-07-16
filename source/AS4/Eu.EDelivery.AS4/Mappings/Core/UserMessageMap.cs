using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using static Eu.EDelivery.AS4.Singletons.AS4Mapper;

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

        private static IEnumerable<Model.Core.PartInfo> MapPartInfos(Xml.PartInfo[] parts)
        {
            if (parts == null || !parts.Any())
            {
                return new Model.Core.PartInfo[0];
            }

            return parts.Select(Map<Model.Core.PartInfo>).Where(p => p != null);
        }

        private static IEnumerable<Model.Core.MessageProperty> MapMessageProperties(Xml.Property[] props)
        {
            if (props == null)
            {
                return Enumerable.Empty<Model.Core.MessageProperty>();
            }

            return props.Where(p => p != null)
                        .Select(Map<Model.Core.MessageProperty>)
                        .Where(p => p != null);
        }

        private void MapXmlToUserMessage()
        {
            CreateMap<Xml.UserMessage, Model.Core.UserMessage>()
                .ConstructUsing(xml => 
                    new Model.Core.UserMessage(
                        messageId: xml.MessageInfo?.MessageId,
                        refToMessageId: xml.MessageInfo?.RefToMessageId,
                        mpc: xml.mpc ?? Constants.Namespaces.EbmsDefaultMpc,
                        collaboration: Map<Model.Core.CollaborationInfo>(xml.CollaborationInfo),
                        sender: Map<Model.Core.Party>(xml.PartyInfo?.From),
                        receiver: Map<Model.Core.Party>(xml.PartyInfo?.To),
                        partInfos: MapPartInfos(xml.PayloadInfo),
                        messageProperties: MapMessageProperties(xml.MessageProperties)))
                .ForMember(dest => dest.MessageId, src => src.MapFrom(t => t.MessageInfo.MessageId))
                .ForMember(dest => dest.RefToMessageId, src => src.MapFrom(t => t.MessageInfo.RefToMessageId))
                .ForMember(dest => dest.Timestamp, src => src.MapFrom(t => t.MessageInfo.Timestamp))
                .ForMember(dest => dest.IsTest, src => src.Ignore())
                .ForMember(dest => dest.IsDuplicate, src => src.Ignore())
                .ForAllOtherMembers(x => x.Ignore());
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
                            From = Map<Xml.From>(modelUserMessage.Receiver),
                            To = Map<Xml.To>(modelUserMessage.Sender)
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