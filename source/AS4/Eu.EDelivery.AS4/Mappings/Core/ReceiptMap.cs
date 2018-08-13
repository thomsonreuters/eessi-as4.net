using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using AutoMapper;
using Eu.EDelivery.AS4.Singletons;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class ReceiptMap : Profile
    {
        private static readonly XmlSerializer NonRepudiationSerializer = 
            new XmlSerializer(typeof(Xml.NonRepudiationInformation));

        public ReceiptMap()
        {
            CreateMap<Model.Core.Receipt, Xml.SignalMessage>()
                .ForMember(dest => dest.Receipt, src => src.MapFrom(t => t))
                .ForMember(dest => dest.MessageInfo, src => src.MapFrom(t => t))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Model.Core.Receipt, Xml.Receipt>()
                .ForMember(dest => dest.UserMessage, src => src.MapFrom(t => t.UserMessage))
                .ForMember(dest => dest.NonRepudiationInformation, src => src.MapFrom(t => t.NonRepudiationInformation))
               .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Xml.SignalMessage, Model.Core.Receipt>()
                .ConstructUsing((xml, ctx) =>
                {
                    string messageId = xml.MessageInfo?.MessageId;
                    string refToMessageId = xml.MessageInfo?.RefToMessageId;
                    DateTimeOffset timestamp = xml.MessageInfo?.Timestamp ?? default(DateTimeOffset);

                    Maybe<Xml.RoutingInputUserMessage> routingM = GetRoutingFromMapperOptions(ctx.Items);
                    Maybe<Model.Core.NonRepudiationInformation> nriM = GetNonRepudiationFromXml(xml.Receipt);
                    Maybe<Model.Core.UserMessage> userM = GetUserMessageFromXml(xml.Receipt);

                    var routingNriReceiptM = 
                        routingM.Zip(nriM, (routing, nri) => new Model.Core.Receipt(messageId, refToMessageId, timestamp, nri, routing));

                    var routingUserReceiptM = 
                        routingM.Zip(userM, (routing, user) => new Model.Core.Receipt(messageId, refToMessageId, timestamp, user, routing));

                    var routingReceipt = routingM.Select(routing => new Model.Core.Receipt(messageId, refToMessageId, timestamp, routing));
                    var nriReceipt = nriM.Select(nri => new Model.Core.Receipt(messageId, refToMessageId, timestamp, nri));
                    var userReceipt = userM.Select(user => new Model.Core.Receipt(messageId, refToMessageId, timestamp, user));

                    return routingNriReceiptM
                        .OrElse(routingUserReceiptM)
                        .OrElse(routingReceipt)
                        .OrElse(nriReceipt)
                        .OrElse(userReceipt)
                        .GetOrElse(() => new Model.Core.Receipt(messageId, refToMessageId, timestamp));

                }).ForAllOtherMembers(t => t.Ignore());
        }

        private static Maybe<Model.Core.NonRepudiationInformation> GetNonRepudiationFromXml(Xml.Receipt r)
        {
            XmlElement firstNrrElement = r.Any?.FirstOrDefault();

            if (firstNrrElement != null
                && firstNrrElement.LocalName.IndexOf(
                    "NonRepudiationInformation",
                    StringComparison.OrdinalIgnoreCase) > -1)
            {
                object deserialize = NonRepudiationSerializer.Deserialize(new XmlNodeReader(firstNrrElement));
                var nonRepudiation = AS4Mapper.Map<Model.Core.NonRepudiationInformation>(deserialize);
                return Maybe.Just(nonRepudiation);
            }

            if (r.NonRepudiationInformation != null)
            {
                var nonRepudiation = AS4Mapper.Map<Model.Core.NonRepudiationInformation>(
                    r.NonRepudiationInformation);
                
                return Maybe.Just(nonRepudiation);
            }

            return Maybe<Model.Core.NonRepudiationInformation>.Nothing;
        }

        private static Maybe<Model.Core.UserMessage> GetUserMessageFromXml(Xml.Receipt r)
        {
            if (r.UserMessage == null)
            {
                return Maybe.Nothing<Model.Core.UserMessage>();
            }

            var userMessage = AS4Mapper.Map<Model.Core.UserMessage>(r.UserMessage);
            return Maybe.Just(userMessage);
        }

        private static Maybe<Xml.RoutingInputUserMessage> GetRoutingFromMapperOptions(IDictionary<string, object> options)
        {
            if (options.TryGetValue(Model.Core.SignalMessage.RoutingInputKey, out object value))
            {
                var routing = (Xml.RoutingInputUserMessage) value;
                return Maybe.Just(routing);
            }

            return Maybe.Nothing<Xml.RoutingInputUserMessage>();
        }
    }
}
