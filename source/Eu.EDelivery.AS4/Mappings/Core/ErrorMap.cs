using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Xml;
using Error = Eu.EDelivery.AS4.Model.Core.Error;
using SignalMessage = Eu.EDelivery.AS4.Xml.SignalMessage;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    internal class ErrorMap
    {
        /// <summary>
        /// Maps from a XML representation with optional routing usermessage to a domain model representation of an AS4 error.
        /// </summary>
        /// <param name="xml">The XML representation to convert.</param>
        /// <param name="routing">The optional routing usermessage to include in the to be created error.</param>
        internal static Error Convert(SignalMessage xml, Maybe<RoutingInputUserMessage> routing)
        {
            if (xml == null)
            {
                throw new ArgumentNullException(nameof(xml));
            }

            if (routing == null)
            {
                throw new ArgumentNullException(nameof(routing));
            }

            if (xml.Error == null)
            {
                throw new ArgumentException(
                    @"Cannot create Error domain model from a XML representation without an Error element",
                    nameof(xml.Error));
            }

            string messageId = xml.MessageInfo?.MessageId;
            string refToMessageId = xml.MessageInfo?.RefToMessageId;
            DateTimeOffset timestamp = xml.MessageInfo?.Timestamp.ToDateTimeOffset() ?? DateTimeOffset.Now;

            IEnumerable<ErrorLine> lines =
                (xml.Error ?? new Xml.Error[0])
                .Where(l => l != null)
                .Select(l => new ErrorLine(
                    GetErrorCodeFromXml(l.errorCode),
                    l.severity.ToEnum(Severity.FAILURE),
                    l.shortDescription.ToEnum(ErrorAlias.Other),
                    l.origin.AsMaybe(),
                    l.category.AsMaybe(),
                    l.refToMessageInError.AsMaybe(),
                    l.Description.AsMaybe().Select(d => new ErrorDescription(d.lang, d.Value)),
                    l.ErrorDetail.AsMaybe()))
                .ToArray();

            return routing.Select(r => new Error(messageId, refToMessageId, timestamp, lines, r))
                          .GetOrElse(() => new Error(messageId, refToMessageId, timestamp, lines));
        }

        /// <summary>
        /// Maps from a domain model representation to a XML representation of an AS4 error.
        /// </summary>
        /// <param name="model">The domain model to convert.</param>
        internal static SignalMessage Convert(Error model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            Xml.Error MapErrorLine(ErrorLine l)
            {
                return new Xml.Error
                {
                    errorCode = l.ErrorCode.GetString(),
                    severity = l.Severity.ToString(),
                    origin = l.Origin.GetOrElse(() => null),
                    category = l.Category.GetOrElse(() => null),
                    refToMessageInError = l.RefToMessageInError.GetOrElse(() => null),
                    shortDescription = l.ShortDescription.ToString(),
                    ErrorDetail = l.Detail.GetOrElse(() => null),
                    Description = l.Description
                                   .Select(d => new Description { lang = d.Language, Value = d.Value })
                                   .GetOrElse(() => null)
                };
            }

            return new SignalMessage
            {
                MessageInfo = new MessageInfo
                {
                    MessageId = model.MessageId,
                    RefToMessageId = model.RefToMessageId,
                    Timestamp = model.Timestamp.LocalDateTime
                },
                Error = model.ErrorLines.Select(MapErrorLine).ToArray()
            };
        }

        private static ErrorCode GetErrorCodeFromXml(string errorCodeXml)
        {
            if (errorCodeXml == null)
            {
                return ErrorCode.Ebms0004;
            }

            return errorCodeXml
                   .ToUpper()
                   .Replace("EBMS:", string.Empty)
                   .ToEnum(ErrorCode.Ebms0004);
        }
    }
}