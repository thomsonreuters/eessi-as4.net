using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Singletons;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// <see cref="SubmitMessage"/> Resolver to get the <see cref="PartInfo"/> Models
    /// </summary>
    public class SubmitPayloadInfoResolver : ISubmitResolver<List<PartInfo>>
    {
        public static readonly SubmitPayloadInfoResolver Default = new SubmitPayloadInfoResolver();

        /// <summary>
        /// Resolve the <see cref="PartyInfo"/>
        /// 1. SubmitMessage / Payloads / Payload[n] / Id
        /// 2. Generated according to Settings / GuidFormat
        /// </summary>
        /// <param name="submitMessage"></param>
        /// <returns></returns>
        public List<PartInfo> Resolve(SubmitMessage submitMessage)
        {
            if (submitMessage.Payloads == null)
            {
                return new List<PartInfo>();
            }

            return submitMessage.Payloads.Select(p => CreatePartInfo(p, submitMessage)).ToList();
        }

        private static PartInfo CreatePartInfo(Payload submitPayload, SubmitMessage submit)
        {
            string href = submitPayload.Id ?? IdentifierFactory.Instance.Create();
            
            var returnPayload = new PartInfo(href.StartsWith("cid:") ? href : $"cid:{href}");

            if (submitPayload.Schemas != null)
            {
                returnPayload.Schemas = submitPayload.Schemas
                    .Select(AS4Mapper.Map<Model.Core.Schema>)
                    .ToList();
            }

            if (submitPayload.PayloadProperties != null)
            {
                returnPayload.Properties = submitPayload.PayloadProperties
                    .Select(prop => (prop.Name, prop.Value))
                    .Concat(CompressionProperties(submitPayload, submit))
                    .ToDictionary<(string propName, string propValue), string, string>(t => t.propName, t => t.propValue);
            }

            return returnPayload;
        }


        private static IEnumerable<(string propName, string propValue)> CompressionProperties(
            Payload payload, 
            SubmitMessage submitMessage)
        {
            if (submitMessage.PMode.MessagePackaging.UseAS4Compression)
            {
                return new[]
                {
                    ("CompressionType", "application/gzip"),
                    ("MimeType", !string.IsNullOrEmpty(payload.MimeType)
                        ? payload.MimeType
                        : "application/octet-stream")
                };
            }

            return Enumerable.Empty<(string, string)>();
        }
    }
}
