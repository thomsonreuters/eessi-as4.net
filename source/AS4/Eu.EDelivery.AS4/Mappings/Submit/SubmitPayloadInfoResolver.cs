using System.Collections.Generic;
using System.IO;
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
                return Enumerable.Empty<PartInfo>().ToList();
            }

            return ResolvePartInfosFromSubmitMessage(submitMessage).ToList();
        }

        private static IEnumerable<PartInfo> ResolvePartInfosFromSubmitMessage(SubmitMessage submitMessage)
        {
            bool submitContainsDuplicatePayloadIds = 
                submitMessage.Payloads.GroupBy(p => p.Id).All(g => g.Count() == 1) == false;

            if (submitContainsDuplicatePayloadIds)
            {
                throw new InvalidDataException("Invalid Payloads: duplicate Payload Ids");
            }

            return submitMessage.Payloads.Select(CreatePartInfo);
        }

        private static PartInfo CreatePartInfo(Payload submitPayload)
        {
            string href = submitPayload.Id ?? IdentifierFactory.Instance.Create();

            var returnPayload = new PartInfo(href.StartsWith("cid:") ? href : $"cid:{href}");

            if (submitPayload.Schemas != null)
            {
                returnPayload.Schemas = submitPayload.Schemas
                    .Select(SubmitToAS4Schema)
                    .ToList();
            }

            if (submitPayload.PayloadProperties != null)
            {
                returnPayload.Properties = submitPayload.PayloadProperties
                    .Select(SubmitToProperty)
                    .Concat(CompressionProperties(submitPayload))
                    .ToDictionary(t => t.name, t => t.value);
            }

            return returnPayload;
        }

        private static Model.Core.Schema SubmitToAS4Schema(Model.Common.Schema sch)
        {
            var schema = AS4Mapper.Map<Model.Core.Schema>(sch);
            if (string.IsNullOrEmpty(schema.Location))
            {
                throw new InvalidDataException("Invalid Schema: Schema needs a location");
            }

            return schema;
        }

        private static (string name, string value) SubmitToProperty(PayloadProperty prop)
        {
            if (string.IsNullOrEmpty(prop.Name))
            {
                throw new InvalidDataException("Invalid Payload Property: Property requires name");
            }

            return (prop.Name, prop.Value);
        }

        private static IEnumerable<(string name, string value)> CompressionProperties(Payload submit)
        {
            return new[]
            {
                ("CompressionType", "application/gzip"),
                ("MimeType", !string.IsNullOrEmpty(submit.MimeType) ? submit.MimeType : "application/octet-stream")
            };
        }
    }
}
