using System;
using AutoMapper;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    public class ErrorMap : Profile
    {
        public ErrorMap()
        {
            MapErrorToXml();
            MapXmlToError();
        }

        private void MapErrorToXml()
        {
            CreateMap<Xml.SignalMessage, Error>()
                .ForMember(dest => dest.MessageId, src => src.MapFrom(x => x.MessageInfo.MessageId))
                .ForMember(dest => dest.RefToMessageId, src => src.MapFrom(x => x.MessageInfo.RefToMessageId))
                .ForMember(dest => dest.Timestamp, src => src.MapFrom(x => x.MessageInfo.Timestamp))
                .ForMember(dest => dest.Errors, src => src.MapFrom(t => t.Error))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Xml.Error, ErrorDetail>()
                .ForMember(dest => dest.ErrorCode, src => src.MapFrom(t => t.errorCode))
                .ForMember(dest => dest.Severity, src => src.MapFrom(t => MapToSeverityEnum(t)))
                .ForMember(dest => dest.Origin, src => src.MapFrom(t => t.origin))
                .ForMember(dest => dest.Category, src => src.MapFrom(t => t.category))
                .ForMember(dest => dest.RefToMessageInError, src => src.MapFrom(t => t.refToMessageInError))
                .ForMember(dest => dest.ShortDescription, src => src.MapFrom(t => t.shortDescription))
                .ForMember(dest => dest.Detail, src => src.MapFrom(t => t.ErrorDetail))
                .ForAllOtherMembers(x => x.Ignore());
        }

        private Severity MapToSeverityEnum(Xml.Error xmlError)
        {
            var severity = Severity.FAILURE;
            Enum.TryParse(xmlError.severity, out severity);
            return severity;
        }

        private void MapXmlToError()
        {
            CreateMap<Error, Xml.SignalMessage>()
                .ForMember(dest => dest.MessageInfo, src => src.MapFrom(t => t))
                .ForMember(dest => dest.Error, src => src.MapFrom(t => t.Errors))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<ErrorDetail, Xml.Error>()
                .ForMember(dest => dest.errorCode, src => src.MapFrom(t => t.ErrorCode))
                .ForMember(dest => dest.severity, src => src.MapFrom(t => t.Severity))
                .ForMember(dest => dest.origin, src => src.MapFrom(t => t.Origin))
                .ForMember(dest => dest.category, src => src.MapFrom(t => t.Category))
                .ForMember(dest => dest.refToMessageInError, src => src.MapFrom(t => t.RefToMessageInError))
                .ForMember(dest => dest.shortDescription, src => src.MapFrom(t => t.ShortDescription))
                .ForMember(dest => dest.ErrorDetail, src => src.MapFrom(t => t.Detail))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}