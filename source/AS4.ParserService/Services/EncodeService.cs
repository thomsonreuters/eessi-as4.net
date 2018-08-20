using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AS4.ParserService.Infrastructure;
using AS4.ParserService.Models;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Singletons;

namespace AS4.ParserService.Services
{
    internal class EncodeService
    {
        internal async Task<MessagingContext> CreateAS4Message(EncodeMessageInfo encodeInfo)
        {
            var pmode = await AssembleSendingPMode(encodeInfo);

            if (pmode == null)
            {
                return null;
            }

            var as4Message = AssembleAS4Message(pmode, encodeInfo.Payloads);

            var context = SetupMessagingContext(as4Message, pmode);

            return await StepProcessor.ExecuteStepsAsync(context, StepRegistry.GetOutboundProcessingStepConfiguration());
        }

        private static async Task<SendingProcessingMode> AssembleSendingPMode(EncodeMessageInfo encodeInfo)
        {
            var pmode = await Deserializer.ToSendingPMode(encodeInfo.SendingPMode);

            if (pmode == null)
            {
                return null;
            }

            if (pmode.Security?.Signing?.IsEnabled ?? false)
            {
                pmode.Security.Signing.SigningCertificateInformation = new PrivateKeyCertificate
                {
                    Certificate = Convert.ToBase64String(encodeInfo.SigningCertificate ?? new byte[] { }),
                    Password = encodeInfo.SigningCertificatePassword
                };
            }

            if (pmode.Security?.Encryption?.IsEnabled ?? false)
            {
                pmode.Security.Encryption.EncryptionCertificateInformation = new PublicKeyCertificate
                {
                    Certificate = Convert.ToBase64String(encodeInfo.EncryptionPublicKeyCertificate ?? new byte[] { })
                };
            }

            return pmode;
        }

        private static AS4Message AssembleAS4Message(SendingProcessingMode pmode, IEnumerable<PayloadInfo> payloads)
        {
            var submitMessage = new SubmitMessage();
            submitMessage.PMode = pmode;

            var submitPayloads = new List<Payload>();

            foreach (var p in payloads)
            {
                submitPayloads.Add(new Payload(p.PayloadName, "", p.ContentType));
            }

            submitMessage.Payloads = submitPayloads.ToArray();

            var userMessage = AS4Mapper.Map<UserMessage>(submitMessage);

            var as4Message = AS4Message.Create(userMessage);

            foreach (var payload in payloads)
            {
                as4Message.AddAttachment(
                    new Attachment(
                        payload.PayloadName,
                        new MemoryStream(payload.Content),
                        payload.ContentType));
            }

            return as4Message;
        }

        private static MessagingContext SetupMessagingContext(AS4Message as4Message, SendingProcessingMode sendingPMode)
        {
            var context = new MessagingContext(as4Message, MessagingContextMode.Submit);
            context.SendingPMode = sendingPMode;

            return context;
        }
    }
}