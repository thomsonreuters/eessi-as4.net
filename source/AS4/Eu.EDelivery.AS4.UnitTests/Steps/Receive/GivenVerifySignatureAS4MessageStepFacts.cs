using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.Streaming;
using Eu.EDelivery.AS4.TestUtils;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;
using CryptoReference = System.Security.Cryptography.Xml.Reference;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing the <see cref="VerifySignatureAS4MessageStep" />
    /// </summary>
    public class GivenVerifySignatureAS4MessageStepFacts : GivenDatastoreFacts
    {
        private const string ContentType =
            "multipart/related; boundary=\"=-dXYE+NJdacou7AbmYZgUPw==\"; type=\"application/soap+xml\"; charset=\"utf-8\"";

        private readonly VerifySignatureAS4MessageStep _step;
        private MessagingContext _messagingContext;

        public GivenVerifySignatureAS4MessageStepFacts()
        {
            _step = new VerifySignatureAS4MessageStep();
        }

        public class GivenValidArguments : GivenVerifySignatureAS4MessageStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepSucceedsAsync()
            {
                // Arrange
                _messagingContext = await GetSignedInternalMessageAsync(Properties.Resources.as4_soap_signed_message);

                UsingAllowedSigningVerification();

                // Act
                StepResult result = await _step.ExecuteAsync(_messagingContext, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.MessagingContext.AS4Message.IsSigned);
            }

            [Fact]
            public async Task ThenExecuteStepSuceeds_IfNRRHashesAreEqual()
            {
                // Arrange
                const string messageId = "verify-nrr-message-id";

                AS4Message signedUserMessage = await SignedUserMessage(messageId);
                InsertOutMessageWithLocation(messageId, signedUserMessage.ContentType);

                AS4Message signedReceiptResult = NRRReceiptHashes(messageId, signedUserMessage, hashes => hashes);
                StubMessageBodyRetriever messageStore = StubMessageStoreThatRetreives(signedUserMessage);

                Stream stream = await messageStore.LoadMessageBodyAsync(null);
                var serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());
                AS4Message as4Message = await serializer.DeserializeAsync(stream, signedUserMessage.ContentType, CancellationToken.None);

                // Act
                StepResult verifyResult = await ExerciseVerifyNRRReceipt(messageStore, signedReceiptResult);

                // Assert
                Assert.True(verifyResult.CanProceed);
            }
        }

        public class GivenInvalidArguments : GivenVerifySignatureAS4MessageStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepFailsAsync()
            {
                // Arrange
                _messagingContext =
                    await GetSignedInternalMessageAsync(Properties.Resources.as4_soap_wrong_signed_message);

                UsingAllowedSigningVerification();

                // Act
                StepResult result = await _step.ExecuteAsync(_messagingContext, CancellationToken.None);

                // Assert
                ErrorResult error = result.MessagingContext.ErrorResult;
                Assert.Equal(ErrorCode.Ebms0101, error.Code);
            }

            [Fact]
            public async Task ThenExecuteStepFailsWithUntrustedCertificateAsync()
            {
                // Arrange
                _messagingContext =
                    await GetSignedInternalMessageAsync(Properties.Resources.as4_soap_untrusted_signed_message);

                UsingAllowedSigningVerification();

                // Act
                StepResult result = await _step.ExecuteAsync(_messagingContext, CancellationToken.None);

                // Assert
                ErrorResult error = result.MessagingContext.ErrorResult;
                Assert.Equal(ErrorCode.Ebms0101, error.Code);
            }

            [Fact]
            public async Task ThenExecuteStepFailsWithUnmatchingRepudiationHashes()
            {
                // Arrange
                const string messageId = "verify-nrr-message-id";

                AS4Message signedUserMessage = await SignedUserMessage(messageId);
                InsertOutMessageWithLocation(messageId, signedUserMessage.ContentType);

                AS4Message signedReceiptResult = NRRReceiptHashes(messageId, signedUserMessage, hashes => hashes.Reverse().ToArray());
                StubMessageBodyRetriever messageStore = StubMessageStoreThatRetreives(signedUserMessage);

                // Act
                StepResult verifyResult = await ExerciseVerifyNRRReceipt(messageStore, signedReceiptResult);

                // Assert
                Assert.False(verifyResult.CanProceed);
                Assert.Equal(ErrorCode.Ebms0101, verifyResult.MessagingContext.ErrorResult.Code);
            }
        }

        protected async Task<StepResult> ExerciseVerifyNRRReceipt(IAS4MessageBodyStore messageStore, AS4Message signedReceiptResult)
        {
            var verifyNrrPMode = new SendingProcessingMode { ReceiptHandling = { VerifyNRR = true } };
            var verifySignaturePMode = new ReceivingProcessingMode { Security = { SigningVerification = { Signature = Limit.Required } } };

            var step = new VerifySignatureAS4MessageStep(
                GetDataStoreContext,
                StubConfig.Default,
                messageStore);

            return await step.ExecuteAsync(
                new MessagingContext(
                    signedReceiptResult,
                    MessagingContextMode.Receive)
                {
                    SendingPMode = verifyNrrPMode,
                    ReceivingPMode = verifySignaturePMode
                },
                CancellationToken.None);
        }

        protected static AS4Message NRRReceiptHashes(
            string messageId,
            AS4Message signedUserMessage,
            Func<byte[], byte[]> adaptHashes)
        {
            var references = signedUserMessage.SecurityHeader.GetReferences()
                .Cast<CryptoReference>()
                .Select(r => new Reference
                {
                    URI = r.Uri,
                    DigestMethod = new ReferenceDigestMethod(),
                    Transforms = new List<ReferenceTransform>(),
                    DigestValue = adaptHashes(r.DigestValue)

                })
                .Select(r => new MessagePartNRInformation { Reference = r })
                .ToList();

            var receipt = new Receipt
            {
                RefToMessageId = messageId,
                NonRepudiationInformation = new NonRepudiationInformation
                {
                    MessagePartNRInformation = references
                }
            };


            return AS4MessageUtils.SignWithCertificate(AS4Message.Create(receipt),
                                                       new StubCertificateRepository().GetStubCertificate());
        }

        protected void InsertOutMessageWithLocation(string messageId, string contentType)
        {
            using (DatastoreContext context = GetDataStoreContext())
            {
                var repo = new DatastoreRepository(context);
                repo.InsertOutMessage(new OutMessage(messageId) { MessageLocation = messageId, ContentType = contentType });
                context.SaveChanges();
            }
        }

        private static StubMessageBodyRetriever StubMessageStoreThatRetreives(AS4Message signedUserMessage)
        {
            return new StubMessageBodyRetriever(() =>
            {
                var serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());
                var memory = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk);
                serializer.Serialize(signedUserMessage, memory, CancellationToken.None);
                memory.Position = 0;

                return memory;
            });
        }

        private static Task<AS4Message> SerializeDeserializeMime(AS4Message msg)
        {
            var serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());
            var memory = new MemoryStream();
            serializer.Serialize(msg, memory, CancellationToken.None);
            memory.Position = 0;
            return serializer.DeserializeAsync(memory, msg.ContentType, CancellationToken.None);
        }

        private static async Task<AS4Message> SignedUserMessage(string messageId)
        {
            AS4Message userMessage = AS4Message.Create(new UserMessage(messageId));
            userMessage.AddAttachment(new FilledAttachment());
            userMessage = await SerializeDeserializeMime(userMessage);

            return AS4MessageUtils.SignWithCertificate(userMessage, new StubCertificateRepository().GetStubCertificate());
        }

        private static SendingProcessingMode SigningPMode()
        {
            return new SendingProcessingMode
            {
                Security =
                    {
                        Signing =
                        {
                            IsEnabled = true,
                            SigningCertificateInformation = new CertificateFindCriteria
                            {
                                CertificateFindType = X509FindType.FindByThumbprint,
                                CertificateFindValue = new StubCertificateRepository().GetStubCertificate().Thumbprint
                            }
                        }
                    }
            };
        }

        protected async Task<MessagingContext> GetSignedInternalMessageAsync(string xml)
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            var serializer = new SoapEnvelopeSerializer();
            AS4Message as4Message = await serializer.DeserializeAsync(memoryStream, ContentType, CancellationToken.None);

            return new MessagingContext(as4Message, MessagingContextMode.Unknown);
        }

        protected void UsingAllowedSigningVerification()
        {
            var receivingPMode = new ReceivingProcessingMode();
            receivingPMode.Security.SigningVerification.Signature = Limit.Allowed;
            _messagingContext.ReceivingPMode = receivingPMode;
        }
    }
}