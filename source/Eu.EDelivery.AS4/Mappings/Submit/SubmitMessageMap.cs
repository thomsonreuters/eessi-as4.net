using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using AgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using MessageProperty = Eu.EDelivery.AS4.Model.Core.MessageProperty;
using Party = Eu.EDelivery.AS4.Model.Core.Party;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;
using Service = Eu.EDelivery.AS4.Model.Core.Service;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// Collection of mapping functions to create ebMS models from Submit models,
    /// optionally forwarding calls to mapping from <see cref="Model.PMode.SendingProcessingMode"/> models.
    /// </summary>
    internal static class SubmitMessageMap
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="submit"></param>
        /// <param name="sendingPMode"></param>
        /// <returns></returns>
        internal static UserMessage CreateUserMessage(SubmitMessage submit, SendingProcessingMode sendingPMode)
        {
            if (submit == null)
            {
                throw new ArgumentNullException(nameof(submit));
            }

            if (sendingPMode == null)
            {
                throw new ArgumentNullException(nameof(sendingPMode));
            }

            var collaboration = new CollaborationInfo(
                ResolveAgreement(submit, sendingPMode),
                ResolveService(submit, sendingPMode),
                ResolveAction(submit, sendingPMode),
                ResolveConversationId(submit));

            return new UserMessage(
                messageId: submit.MessageInfo?.MessageId ?? IdentifierFactory.Instance.Create(),
                refToMessageId: submit.MessageInfo?.RefToMessageId,
                timestamp: DateTimeOffset.Now,
                mpc: ResolveMpc(submit, sendingPMode),
                collaboration: collaboration,
                sender: ResolveSenderParty(submit, sendingPMode),
                receiver: ResolveReceiverParty(submit, sendingPMode),
                partInfos: ResolvePartInfos(submit, sendingPMode),
                messageProperties: ResolveMessageProperties(submit, sendingPMode));
        }

        private static string ResolveAction(SubmitMessage submit, SendingProcessingMode sendingPMode)
        {
            string submitAction = submit?.Collaboration?.Action;
            string pmodeAction = sendingPMode?.MessagePackaging?.CollaborationInfo?.Action;

            if (sendingPMode?.AllowOverride == false
                && !String.IsNullOrEmpty(submitAction)
                && !String.IsNullOrEmpty(pmodeAction)
                && !StringComparer.OrdinalIgnoreCase.Equals(submitAction, pmodeAction))
            {
                throw new NotSupportedException(
                    $"SubmitMessage is not allowed by SendingPMode {sendingPMode.Id} to override Action");
            }

            if (!String.IsNullOrEmpty(submitAction))
            {
                return submit.Collaboration.Action;
            }

            return SendingPModeMap.ResolveAction(sendingPMode);
        }

        private static string ResolveConversationId(SubmitMessage submit)
        {
            string submitConversationId = submit?.Collaboration?.ConversationId;
            return String.IsNullOrEmpty(submitConversationId)
                ? CollaborationInfo.DefaultConversationId
                : submitConversationId;
        }

        private static Maybe<AgreementReference> ResolveAgreement(SubmitMessage submit, SendingProcessingMode sendingPMode)
        {
            var pmodeAgreement = sendingPMode?.MessagePackaging?.CollaborationInfo?.AgreementReference;
            var submitAgreement = submit?.Collaboration?.AgreementRef;

            bool includePModeId = sendingPMode?.MessagePackaging?.IncludePModeId == true;

            if (sendingPMode?.AllowOverride == false
                && !String.IsNullOrEmpty(submitAgreement?.Value)
                && !String.IsNullOrEmpty(pmodeAgreement?.Value)
                && !StringComparer.OrdinalIgnoreCase.Equals(pmodeAgreement.Value, submitAgreement.Value))
            {
                throw new NotSupportedException(
                    $"SubmitMessage is not allowed by the Sending PMode {sendingPMode.Id} to override AgreementReference.Value");
            }

            if (!String.IsNullOrEmpty(submitAgreement?.Value))
            {
                return Maybe.Just(
                    new AgreementReference(
                        submitAgreement.Value,
                        submitAgreement.RefType,
                        includePModeId ? sendingPMode.Id : null));
            }

            if (!String.IsNullOrEmpty(pmodeAgreement?.Value))
            {
                return Maybe.Just(
                    new AgreementReference(
                        pmodeAgreement.Value,
                        pmodeAgreement.Type,
                        includePModeId ? sendingPMode.Id : null));
            }

            return Maybe<AgreementReference>.Nothing;
        }

        private static Service ResolveService(SubmitMessage submit, SendingProcessingMode sendingPMode)
        {
            var pmodeService = sendingPMode?.MessagePackaging?.CollaborationInfo?.Service;
            var submitService = submit?.Collaboration?.Service;

            if (sendingPMode?.AllowOverride == false
                && !String.IsNullOrEmpty(submitService?.Value)
                && !String.IsNullOrEmpty(pmodeService?.Value)
                && !StringComparer.OrdinalIgnoreCase.Equals(submitService.Value, pmodeService.Value))
            {
                throw new NotSupportedException(
                    $"SubmitMessage is not allowed by SendingPMode {sendingPMode.Id} to override CollaborationInfo.Service");
            }

            if (submitService?.Value != null)
            {
                return new Service(submitService.Value, submitService.Type);
            }

            return SendingPModeMap.ResolveService(sendingPMode);
        }

        private static IEnumerable<MessageProperty> ResolveMessageProperties(
            SubmitMessage submit,
            SendingProcessingMode sendingPMode)
        {
            if (submit.MessageProperties != null)
            {
                foreach (Model.Common.MessageProperty p in submit.MessageProperties)
                {
                    yield return new MessageProperty(p?.Name, p?.Value, p?.Type);
                }
            }

            if (sendingPMode.MessagePackaging?.MessageProperties != null)
            {
                foreach (Model.PMode.MessageProperty p in sendingPMode.MessagePackaging.MessageProperties)
                {
                    yield return new MessageProperty(p?.Name, p?.Value, p?.Type);
                }
            }
        }

        private static string ResolveMpc(SubmitMessage submit, SendingProcessingMode sendingPMode)
        {
            string pmodeMpc = sendingPMode?.MessagePackaging?.Mpc;
            string submitMpc = submit?.MessageInfo?.Mpc;

            if (sendingPMode?.AllowOverride == false
                && !String.IsNullOrEmpty(submitMpc)
                && !StringComparer.OrdinalIgnoreCase.Equals(Constants.Namespaces.EbmsDefaultMpc, submitMpc)
                && !String.IsNullOrEmpty(pmodeMpc)
                && !StringComparer.OrdinalIgnoreCase.Equals(submitMpc, pmodeMpc))
            {
                throw new NotSupportedException(
                    $"SubmitMessage is not allowed by SendingPMode {sendingPMode.Id} to override Mpc");
            }

            return !String.IsNullOrEmpty(submitMpc)
                ? submitMpc
                : !String.IsNullOrEmpty(pmodeMpc)
                    ? pmodeMpc
                    : Constants.Namespaces.EbmsDefaultMpc;
        }

        private static Party ResolveReceiverParty(SubmitMessage submit, SendingProcessingMode sendingPMode)
        {
            var pmodeParty = sendingPMode?.MessagePackaging?.PartyInfo?.ToParty;
            var submitParty = submit?.PartyInfo?.ToParty;

            if (sendingPMode?.AllowOverride == false
                && submitParty != null
                && pmodeParty != null
                && !submitParty.Equals(pmodeParty))
            {
                throw new NotSupportedException(
                    $"SubmitMessage is not allowed by the SendingPMode {sendingPMode.Id} to override Receiver Party");
            }

            if (submitParty != null)
            {
                var ids = submitParty.PartyIds ?? Enumerable.Empty<Model.Common.PartyId>();
                return new Party(submitParty.Role, ids.Select(x => new PartyId(x.Id, x.Type)).ToArray());
            }

            return SendingPModeMap.ResolveReceiver(pmodeParty);
        }

        private static Party ResolveSenderParty(SubmitMessage submit, SendingProcessingMode sendingPMode)
        {
            var pmodeParty = sendingPMode?.MessagePackaging?.PartyInfo?.FromParty;
            var submitParty = submit?.PartyInfo?.FromParty;

            if (sendingPMode?.AllowOverride == false
                && submitParty != null
                && pmodeParty != null
                && !submitParty.Equals(pmodeParty))
            {
                throw new NotSupportedException(
                    $"SubmitMessage is not allowed by SendingPMode {sendingPMode.Id} to override Sender Party");
            }

            if (submitParty != null)
            {
                var ids = submitParty.PartyIds ?? Enumerable.Empty<Model.Common.PartyId>();
                return new Party(submitParty.Role, ids.Select(x => new PartyId(x.Id, x.Type)).ToArray());
            }

            return SendingPModeMap.ResolveSender(pmodeParty);
        }

        private static IEnumerable<PartInfo> ResolvePartInfos(SubmitMessage submit, SendingProcessingMode sendingPMode)
        {
            return (submit?.Payloads ?? Enumerable.Empty<Payload>())
                   .Where(p => p != null)
                   .Select(p => CreatePartInfo(p, sendingPMode))
                   .ToArray();
        }

        private static PartInfo CreatePartInfo(Payload submitPayload, SendingProcessingMode sendingPMode)
        {
            string id = submitPayload.Id ?? IdentifierFactory.Instance.Create();
            string href = id.StartsWith("cid:") ? id : $"cid:{id}";

            IEnumerable<Model.Core.Schema> schemas =
                (submitPayload.Schemas ?? new Model.Common.Schema[0])
                .Where(sch => sch != null)
                .Select(sch =>
                {
                    // TODO: should we throw or skip?
                    if (sch.Location == null)
                    {
                        throw new InvalidDataException(
                            "SubmitMessage contains Payload with a Schema that hasn't got a Location");
                    }

                    return new Model.Core.Schema(sch.Location, sch.Version, sch.Namespace);
                })
                .ToList();

            IDictionary<string, string> properties =
                (submitPayload.PayloadProperties ?? new PayloadProperty[0])
                .Where(p => p != null)
                .Select(prop =>
                {
                    // TODO: should we throw or skip?
                    if (prop.Name == null)
                    {
                        throw new InvalidDataException(
                            "SubmitMessage contains Payload with a PayloadProperty that hasn't got a Name");
                    }

                    return (prop.Name, prop.Value);
                })
                .Concat(CreatePayloadCompressionProperties(submitPayload, sendingPMode))
                .ToDictionary<(string propName, string propValue), string, string>(
                    t => t.propName,
                    t => t.propValue,
                    StringComparer.OrdinalIgnoreCase);

            return new PartInfo(href, properties, schemas);
        }

        private static IEnumerable<(string propName, string propValue)> CreatePayloadCompressionProperties(
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
