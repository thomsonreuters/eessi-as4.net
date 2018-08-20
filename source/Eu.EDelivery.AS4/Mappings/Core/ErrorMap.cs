using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Singletons;
using Error = Eu.EDelivery.AS4.Model.Core.Error;
using SignalMessage = Eu.EDelivery.AS4.Model.Core.SignalMessage;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class ErrorMap : Profile
    {
        public ErrorMap()
        {
            MapErrorToXml();
            MapXmlToError();
        }

        private void MapXmlToError()
        {
            CreateMap<Xml.SignalMessage, Error>()
                .ConstructUsing((xml, ctx) =>
                {
                    string messageId = xml.MessageInfo?.MessageId;
                    string refToMessageId = xml.MessageInfo?.RefToMessageId;
                    DateTimeOffset timestamp = xml.MessageInfo?.Timestamp ?? default(DateTimeOffset);

                    IEnumerable<ErrorLine> lines =
                        (xml.Error ?? new Xml.Error[0]).Select(AS4Mapper.Map<ErrorLine>);

                    if (ctx.Items.TryGetValue(SignalMessage.RoutingInputKey, out object value))
                    {
                        var routing = (Xml.RoutingInputUserMessage) value;
                        return new Error(messageId, refToMessageId, timestamp, lines, routing);
                    }

                    return new Error(messageId, refToMessageId, timestamp, lines);
                })
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Xml.Error, ErrorLine>()
                .ConstructUsing(xml => 
                    new ErrorLine(
                        GetErrorCodeFromXml(xml.errorCode),
                        xml.severity.ToEnum(Severity.FAILURE),
                        xml.shortDescription.ToEnum(ErrorAlias.Other),
                        xml.origin.AsMaybe(),
                        xml.category.AsMaybe(),
                        xml.refToMessageInError.AsMaybe(),
                        xml.Description.AsMaybe().Select(AS4Mapper.Map<ErrorDescription>),
                        xml.ErrorDetail.AsMaybe()))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Xml.Description, ErrorDescription>()
                .ConstructUsing(xml => new ErrorDescription(xml.lang, xml.Value))
                .ForAllOtherMembers(x => x.Ignore());

        }

        private static ErrorCode GetErrorCodeFromXml(string errorCodeXml)
        {
            if (errorCodeXml == null)
            {
                return ErrorCode.Ebms0004;
            }

            return errorCodeXml
                .ToUpper()
                .Replace("EBMS:", String.Empty)
                .ToEnum(ErrorCode.Ebms0004);
        }

        private void MapErrorToXml()
        {
            CreateMap<Error, Xml.SignalMessage>()
                .ForMember(dest => dest.MessageInfo, src => src.MapFrom(t => t))
                .ForMember(dest => dest.Error, src => src.MapFrom(t => t.ErrorLines))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<ErrorLine, Xml.Error>()
                .ForMember(dest => dest.errorCode, src => src.MapFrom(t => t.ErrorCode.GetString()))
                .ForMember(dest => dest.severity, src => src.MapFrom(t => t.Severity))
                .ForMember(dest => dest.origin, src => src.MapFrom(t => t.Origin.GetOrElse(() => null)))
                .ForMember(dest => dest.category, src => src.MapFrom(t => t.Category.GetOrElse(() => null)))
                .ForMember(dest => dest.refToMessageInError, src => src.MapFrom(t => t.RefToMessageInError.GetOrElse(() => null)))
                .ForMember(dest => dest.shortDescription, src => src.MapFrom(t => t.ShortDescription))
                .ForMember(dest => dest.Description, src => src.MapFrom(t => t.Description.GetOrElse(() => null)))
                .ForMember(dest => dest.ErrorDetail, src => src.MapFrom(t => t.Detail.GetOrElse(() => null)))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<ErrorDescription, Xml.Description>()
                .ForMember(dest => dest.lang, src => src.MapFrom(t => t.Language))
                .ForMember(dest => dest.Value, src => src.MapFrom(t => t.Value))
                .ForAllOtherMembers(x => x.Ignore());

        }
    }
}