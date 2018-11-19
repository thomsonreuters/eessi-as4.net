using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AS4.ParserService.Infrastructure;
using AS4.ParserService.Models;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Submit;
using Eu.EDelivery.AS4.Strategies.Retriever;

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

            var as4Message = await AssembleAS4MessageAsync(pmode, encodeInfo.Payloads);

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

        private static async Task<AS4Message> AssembleAS4MessageAsync(SendingProcessingMode pmode, IEnumerable<PayloadInfo> payloads)
        {
            var submitMessage = new SubmitMessage
            {
                PMode = pmode,
                Payloads = payloads.Select(p => new Payload(p.PayloadName, "", p.ContentType)).ToArray()
            };

            var createAS4MessageStep = new CreateAS4MessageStep(
                submitPayload => new InMemoryPayloadRetriever(
                    payloads.First(p => p.PayloadName == submitPayload.Id)));

            var ctx = new MessagingContext(submitMessage) { SendingPMode = pmode };
            StepResult stepResult = await createAS4MessageStep.ExecuteAsync(ctx);
            return stepResult.MessagingContext.AS4Message;
        }

        private static MessagingContext SetupMessagingContext(AS4Message as4Message, SendingProcessingMode sendingPMode)
        {
            var context = new MessagingContext(as4Message, MessagingContextMode.Submit);
            context.SendingPMode = sendingPMode;

            return context;
        }

        private class InMemoryPayloadRetriever : IPayloadRetriever
        {
            private readonly PayloadInfo _payload;

            /// <summary>
            /// Initializes a new instance of the <see cref="InMemoryPayloadRetriever"/> class.
            /// </summary>
            public InMemoryPayloadRetriever(PayloadInfo payload)
            {
                if (payload == null)
                {
                    throw new ArgumentNullException(nameof(payload));
                }

                _payload = payload;
            }

            /// <summary>
            /// Retrieve <see cref="Stream"/> contents from a given <paramref name="location"/>.
            /// </summary>
            /// <param name="location">The location.</param>
            /// <returns></returns>
            public Task<Stream> RetrievePayloadAsync(string location)
            {
                return Task.FromResult<Stream>(new MemoryStream(_payload.Content));
            }
        }
    }
}