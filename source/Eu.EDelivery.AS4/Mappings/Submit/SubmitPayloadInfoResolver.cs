using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// <see cref="SubmitMessage"/> Resolver to get the <see cref="PartInfo"/> Models
    /// </summary>
    internal static class SubmitPayloadInfoResolver
    {
        /// <summary>
        /// Resolve the <see cref="Model.Common.PartyInfo"/>
        /// 1. SubmitMessage / Payloads / Payload[n] / Id
        /// 2. Generated according to Settings / GuidFormat
        /// </summary>
        /// <param name="submitMessage"></param>
        /// <returns></returns>
        public static IEnumerable<PartInfo> Resolve(SubmitMessage submitMessage)
        {
            if (submitMessage == null)
            {
                throw new ArgumentNullException(nameof(submitMessage));
            }

            if (submitMessage.PMode == null)
            {
                throw new ArgumentNullException(nameof(submitMessage.PMode));
            }

            if (submitMessage.Payloads == null)
            {
                return Enumerable.Empty<PartInfo>();
            }

            return submitMessage.Payloads
                .Select(p => CreatePartInfo(p, submitMessage.PMode))
                .ToArray();
        }

        private static PartInfo CreatePartInfo(Payload submitPayload, SendingProcessingMode sendingPMode)
        {
            if (submitPayload == null)
            {
                throw new ArgumentNullException(nameof(submitPayload));
            }

            string id = submitPayload.Id ?? IdentifierFactory.Instance.Create();
            string href = id.StartsWith("cid:") ? id : $"cid:{id}";

            IEnumerable<Model.Core.Schema> schemas = 
                (submitPayload.Schemas ?? new Model.Common.Schema[0])
                .Select(sch =>
                {
                    if (sch == null)
                    {
                        throw new ArgumentNullException(nameof(sch));
                    }

                    if (sch.Location == null)
                    {
                        throw new InvalidOperationException(
                            "SubmitMessage contains Payload with a Schema that hasn't got a Location");
                    }

                    return new Model.Core.Schema(sch.Location, sch.Version, sch.Namespace);
                })
                .ToList();

            IDictionary<string, string> properties =
                (submitPayload.PayloadProperties ?? new PayloadProperty[0])
                .Select(prop =>
                {
                    if (prop == null)
                    {
                        throw new ArgumentNullException(nameof(prop));
                    }

                    if (prop.Name == null)
                    {
                        throw new InvalidOperationException(
                            "SubmitMessage contains Payload with a PayloadProperty that hasn't got a Name");
                    }

                    return (prop.Name, prop.Value);
                })
                .Concat(CompressionProperties(submitPayload, sendingPMode))
                .ToDictionary<(string propName, string propValue), string, string>(
                    t => t.propName, 
                    t => t.propValue,
                    StringComparer.OrdinalIgnoreCase);

            return new PartInfo(href, properties, schemas);
        }

        private static IEnumerable<(string propName, string propValue)> CompressionProperties(
            Payload payload, 
            SendingProcessingMode sendingPMode)
        {
            if (sendingPMode.MessagePackaging?.UseAS4Compression == true)
            {
                return new[]
                {
                    ("CompressionType", "application/gzip"),
                    ("MimeType", !String.IsNullOrEmpty(payload.MimeType)
                        ? payload.MimeType
                        : "application/octet-stream")
                };
            }

            return Enumerable.Empty<(string, string)>();
        }
    }
}
