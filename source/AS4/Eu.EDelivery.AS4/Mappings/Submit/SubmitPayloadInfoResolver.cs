using System.Collections.Generic;
using System.IO;
using AutoMapper;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Utilities;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// <see cref="SubmitMessage"/> Resolver to get the <see cref="PartInfo"/> Models
    /// </summary>
    public class SubmitPayloadInfoResolver : ISubmitResolver<List<PartInfo>>
    {
        /// <summary>
        /// Resolve the <see cref="PartyInfo"/>
        /// 1. SubmitMessage / Payloads / Payload[n] / Id
        /// 2. Generated according to Settings / GuidFormat
        /// </summary>
        /// <param name="submitMessage"></param>
        /// <returns></returns>
        public List<PartInfo> Resolve(SubmitMessage submitMessage)
        {
            var returnPayloads = new List<PartInfo>();
            if (submitMessage.Payloads == null)
            {
                return returnPayloads;
            }

            ResolvePartInfosFromSubmitMessage(submitMessage, returnPayloads);

            return returnPayloads;
        }

        private static void ResolvePartInfosFromSubmitMessage(SubmitMessage submitMessage, List<PartInfo> returnPayloads)
        {
            foreach (Payload submitPayload in submitMessage.Payloads)
            {
                PartInfo returnPayload = CreatePartInfo(submitMessage, submitPayload);
                returnPayloads.Add(returnPayload);
            }
        }

        private static PartInfo CreatePartInfo(SubmitMessage submitMessage, Payload submitPayload)
        {
            string href = submitPayload.Id ?? IdentifierFactory.Instance.Create();

            var returnPayload = new PartInfo(href.StartsWith("cid:") ? href : $"cid:{href}");
            if (submitPayload.Schemas != null)
            {
                InsertSchemasInPayload(submitPayload, returnPayload);
            }

            InsertPropertiesInPayload(submitMessage, submitPayload, returnPayload);
            return returnPayload;
        }

        private static void InsertSchemasInPayload(Payload submitPayload, PartInfo returnPartInfo)
        {
            foreach (Model.Common.Schema submitSchema in submitPayload.Schemas)
            {
                var schema = AS4Mapper.Map<Model.Core.Schema>(submitSchema);
                if (string.IsNullOrEmpty(schema.Location))
                {
                    throw new InvalidDataException("Invalid Schema: Schema needs a location");
                }

                returnPartInfo.Schemas.Add(schema);
            }
        }

        private static void InsertPropertiesInPayload(SubmitMessage message, Payload submitPayload, PartInfo returnPartInfo)
        {
            if (message.PMode.MessagePackaging.UseAS4Compression)
            {
                AddCompressionProperties(submitPayload, returnPartInfo);
            }

            if (submitPayload.PayloadProperties == null)
            {
                return;
            }

            AddPayloadProperties(submitPayload, returnPartInfo);
        }

        private static void AddPayloadProperties(Payload submitPayload, PartInfo returnPartInfo)
        {
            foreach (PayloadProperty payloadProperty in submitPayload.PayloadProperties)
            {
                if (string.IsNullOrEmpty(payloadProperty.Name))
                {
                    throw new InvalidDataException("Invalid Payload Property: Property requires name");
                }

                returnPartInfo.Properties[payloadProperty.Name] = payloadProperty.Value;
            }
        }

        private static void AddCompressionProperties(Payload submitPayload, PartInfo returnPartInfo)
        {
            returnPartInfo.Properties["CompressionType"] = "application/gzip";

            if (!string.IsNullOrEmpty(submitPayload.MimeType))
            {
                returnPartInfo.Properties["MimeType"] = submitPayload.MimeType;
            }
            else
            {
                returnPartInfo.Properties["MimeType"] = "application/octet-stream";
            }
        }
    }
}
