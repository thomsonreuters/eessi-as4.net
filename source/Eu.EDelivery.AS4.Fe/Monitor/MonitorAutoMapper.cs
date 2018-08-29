using System;
using AutoMapper;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Fe.Monitor.Model;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    /// <summary>
    /// Setup automapper for the monitor
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class MonitorAutoMapper : Profile
    {
        private const int ExceptionLength = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonitorAutoMapper"/> class.
        /// </summary>
        public MonitorAutoMapper()
        {
            CreateMap<InMessage, Message>()
                .ForMember(x => x.Status, x => x.MapFrom(y => y.Status))
                .ForMember(x => x.EbmsMessageType, x => x.MapFrom(y => y.EbmsMessageType))
                .ForMember(x => x.Operation, x => x.MapFrom(y => y.Operation))
                .ForMember(x => x.Direction, x => x.UseValue(Direction.Inbound))
                .ForMember(x => x.Mep, x => x.MapFrom(y => y.MEP));
            CreateMap<OutMessage, Message>()
                .ForMember(x => x.Status, x => x.MapFrom(y => y.Status))
                .ForMember(x => x.EbmsMessageType, x => x.MapFrom(y => y.EbmsMessageType))
                .ForMember(x => x.Operation, x => x.MapFrom(y => y.Operation))
                .ForMember(x => x.Direction, x => x.UseValue(Direction.Outbound))
                .ForMember(x => x.Mep, x => x.MapFrom(y => y.MEP));
            CreateMap<InException, ExceptionMessage>()
              .ForMember(x => x.Direction, x => x.UseValue(Direction.Inbound))
              .ForMember(x => x.ExceptionShort, x => x.MapFrom(y => string.IsNullOrEmpty(y.Exception) ? "" : y.Exception.Substring(y.Exception.IndexOf(']') + 1).Split('\r', '\n')[0].Length > ExceptionLength ? y.Exception.Substring(y.Exception.IndexOf(']') + 1).Split('\r', '\n')[0].Substring(0, ExceptionLength) + "..." : y.Exception.Substring(y.Exception.IndexOf(']') + 1).Split('\r', '\n')[0]))
              .ForMember(x => x.HasMessageBody,x => x.MapFrom(y => y.MessageLocation != null && !String.IsNullOrWhiteSpace(y.MessageLocation)));
            CreateMap<OutException, ExceptionMessage>()
              .ForMember(x => x.Direction, x => x.UseValue(Direction.Outbound))
              .ForMember(x => x.ExceptionShort, x => x.MapFrom(y => string.IsNullOrEmpty(y.Exception) ? "" : y.Exception.Substring(y.Exception.IndexOf(']') + 1).Split('\r', '\n')[0].Length > ExceptionLength ? y.Exception.Substring(y.Exception.IndexOf(']') + 1).Split('\r', '\n')[0].Substring(0, ExceptionLength) + "..." : y.Exception.Substring(y.Exception.IndexOf(']') + 1).Split('\r', '\n')[0]))
              .ForMember(x => x.HasMessageBody, x => x.MapFrom(y => y.MessageLocation != null && !String.IsNullOrWhiteSpace(y.MessageLocation)));
        }
    }
}
