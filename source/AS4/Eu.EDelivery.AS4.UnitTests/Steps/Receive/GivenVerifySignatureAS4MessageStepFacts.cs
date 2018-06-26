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
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.Streaming;
using Eu.EDelivery.AS4.TestUtils;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;
using static Eu.EDelivery.AS4.UnitTests.Properties.Resources;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing the <see cref="VerifySignatureAS4MessageStep" />
    /// </summary>
    public class GivenVerifySignatureAS4MessageStepFacts : GivenDatastoreFacts
    {
        public class GivenValidArguments : GivenVerifySignatureAS4MessageStepFacts
        {
            [Fact]
            public async Task Succeeds_Verify_Correct_Signed_UserMessage()
            {
                // Arrange
                MessagingContext ctx = 
                    await DeserializeSignedMessage(as4_soap_signed_message);

                ctx.ReceivingPMode = ReceivingPModeWithAllowedSigningVerification();

                // Act
                StepResult result = await ExerciseVerify(ctx);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.MessagingContext.AS4Message.IsSigned);
            }

            [Fact]
            public async Task Succeeds_Verify_Correct_Signed_Receipt_With_Matching_Repudiation_Hashes()
            {
                // Arrange
                byte[] EqualHashes(byte[] hashes) => hashes;

                // Act
                StepResult verifyResult = await TestVerifyNRRReceipt(EqualHashes);

                // Assert
                Assert.True(verifyResult.CanProceed);
            }

            [Fact]
            public async Task Succeeds_Verify_Receipt_With_Corrupt_Repudiation_Hashes_On_Ignored()
            {
                // Arrange
                byte[] IncrementedHashes(byte[] hashes) => 
                    hashes.Select(i => (byte) (i + 10)).ToArray();

                // Act
                StepResult verifyResult = await TestVerifyNRRReceipt(IncrementedHashes, verifyNrr: false);

                // Assert
                Assert.True(verifyResult.CanProceed);
            }

            [Fact]
            public async Task Succeeds_Verify_Receipt_With_Corrupt_Repudiation_Hashes_If_Receiver_Is_Final_Recipient()
            {
                // Arrange
                byte[] CorruptHashes(byte[] hashes) =>
                    hashes.Select(i => (byte) (i * 10)).ToArray();

                // Act
                StepResult verifyResult = await TestVerifyNRRReceipt(CorruptHashes, intermediary: true);

                // Assert
                Assert.True(verifyResult.CanProceed);
            }

            [Fact]
            public async Task Takes_Sending_PMode_Into_Account_When_Verifies_Non_Multihop_Signal()
            {
                // Arrange
                var as4Msg = AS4Message.Create(new Receipt($"receipt-{Guid.NewGuid()}", $"reftoid-{Guid.NewGuid()}"));
                as4Msg.AddMessageUnit(new UserMessage(messageId: $"user-{Guid.NewGuid()}"));

                var ctx = new MessagingContext(as4Msg, MessagingContextMode.Receive)
                {
                    ReceivingPMode = new ReceivingProcessingMode
                    {
                        Security = { SigningVerification = { Signature = Limit.Required } }
                    },
                    SendingPMode = new SendingProcessingMode
                    {
                        Security = { SigningVerification = { Signature = Limit.Ignored } }
                    }
                };

                // Act
                StepResult result = await ExerciseVerify(ctx);

                // Assert
                Assert.True(result.CanProceed);
            }

            [Fact]
            public async Task Succeeds_Wrong_Signed_SignalMessage_But_Ignored()
            {
                // Arrange
                MessagingContext ctx =
                    await DeserializeSignedMessage(as4_soap_wrong_signed_pullrequest);
                ctx.SendingPMode = new SendingProcessingMode
                {
                    Security = { SigningVerification = { Signature = Limit.Ignored } }
                };

                // Act
                StepResult result = await ExerciseVerify(ctx);

                // Assert
                Assert.True(result.Succeeded);
            }
        }

        public class GivenInvalidArguments : GivenVerifySignatureAS4MessageStepFacts
        {
            [Fact]
            public async Task Fails_Verify_Unsigned_SignalMessage_But_Required()
            {
                // Arrange
                MessagingContext ctx = SignalMessageWithVerification(Limit.Required);

                // Act
                StepResult result = await ExerciseVerify(ctx);

                // Assert
                Assert.False(result.Succeeded);
                Assert.Equal(ErrorAlias.PolicyNonCompliance, result.MessagingContext.ErrorResult.Alias);
            }

            [Fact]
            public async Task Fails_Verify_Signed_SignalMessage_But_Unallowed()
            {
                // Arrange
                MessagingContext ctx = SignalMessageWithVerification(Limit.NotAllowed);
                ctx.AS4Message.Sign(
                    new CalculateSignatureConfig(
                        signingCertificate: new X509Certificate2(
                            rawData: holodeck_partya_certificate,
                            password: certificate_password,
                            keyStorageFlags: X509KeyStorageFlags.Exportable)));

                // Act
                StepResult result = await ExerciseVerify(ctx);

                // Assert
                Assert.False(result.Succeeded);
                Assert.Equal(ErrorAlias.PolicyNonCompliance, result.MessagingContext.ErrorResult.Alias);
            }

            private static MessagingContext SignalMessageWithVerification(Limit sendSignature)
            {
                var signal = AS4Message.Create(new Receipt($"receipt-{Guid.NewGuid()}", $"reftoid-{Guid.NewGuid()}"));
                var ctx = new MessagingContext(signal, MessagingContextMode.Receive)
                {
                    SendingPMode = new SendingProcessingMode
                    {
                        Security = { SigningVerification = { Signature = sendSignature } }
                    }
                };
                return ctx;
            }

            [Fact]
            public async Task Fails_Verify_UserMessage_With_Wrong_Signed_On_Allowed()
            {
                // Arrange
                MessagingContext ctx =
                    await DeserializeSignedMessage(as4_soap_wrong_signed_message);

                ctx.ReceivingPMode = ReceivingPModeWithAllowedSigningVerification();

                // Act
                StepResult result = await ExerciseVerify(ctx);

                // Assert
                ErrorResult error = result.MessagingContext.ErrorResult;
                Assert.Equal(ErrorCode.Ebms0101, error.Code);
            }

            [Fact]
            public async Task Fails_Verify_UserMessage_With_Untrusted_Cert_On_Allowed()
            {
                // Arrange
                MessagingContext ctx =
                    await DeserializeSignedMessage(as4_soap_untrusted_signed_message);

                ctx.ReceivingPMode = ReceivingPModeWithAllowedSigningVerification();

                // Act
                StepResult result = await ExerciseVerify(ctx);

                // Assert
                ErrorResult error = result.MessagingContext.ErrorResult;
                Assert.Equal(ErrorCode.Ebms0101, error.Code);
            }

            [Fact]
            public async Task Fails_Verify_SignalMessage_With_Corrupt_Repidiation_Hashes()
            {
                // Arrange
                byte[] ReversedHashes(byte[] hashes) => hashes.Reverse().ToArray();

                // Act
                StepResult verifyResult = await TestVerifyNRRReceipt(ReversedHashes);

                // Assert
                Assert.False(verifyResult.CanProceed);
                Assert.Equal(ErrorCode.Ebms0101, verifyResult.MessagingContext.ErrorResult.Code);
            }
        }

        protected async Task<StepResult> TestVerifyNRRReceipt(
            Func<byte[], byte[]> adaptHashes, 
            bool verifyNrr = true,
            bool intermediary = false)
        {
            // Arrange
            const string messageId = "verify-nrr-message-id";

            AS4Message signedUserMessage = await SignedUserMessage(messageId);
            InsertOutMessageWithLocation(messageId, signedUserMessage.ContentType, intermediary);

            AS4Message signedReceiptResult = await NRRReceiptHashes(messageId, signedUserMessage, adaptHashes);
            StubMessageBodyRetriever messageStore = StubMessageStoreThatRetreives(signedUserMessage);

            // Act
            return await ExerciseVerifyNRRReceipt(messageStore, signedReceiptResult, verifyNrr);
        }

        private async Task<StepResult> ExerciseVerifyNRRReceipt(
            IAS4MessageBodyStore messageStore, 
            AS4Message signedReceiptResult, 
            bool verifyNrr)
        {
            var verifyNrrPMode = new SendingProcessingMode { ReceiptHandling = { VerifyNRR = verifyNrr } };
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
                });
        }

        protected static async Task<AS4Message> NRRReceiptHashes(
            string messageId,
            AS4Message signedUserMessage,
            Func<byte[], byte[]> adaptHashes)
        {
            var references = signedUserMessage.SecurityHeader.GetReferences()
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

            return await SerializeDeserializeSoap(
                AS4MessageUtils.SignWithCertificate(
                    AS4Message.Create(receipt), 
                    new StubCertificateRepository().GetStubCertificate()));
        }

        protected void InsertOutMessageWithLocation(
            string messageId, 
            string contentType, 
            bool intermediary)
        {
            using (DatastoreContext context = GetDataStoreContext())
            {
                var repo = new DatastoreRepository(context);
                repo.InsertOutMessage(new OutMessage(messageId)
                {
                    MessageLocation = messageId,
                    ContentType = contentType,
                    Intermediary = intermediary
                });
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

        private static Task<AS4Message> SerializeDeserializeSoap(AS4Message msg)
        {
            var serializer = new SoapEnvelopeSerializer();
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

        protected async Task<MessagingContext> DeserializeSignedMessage(string xml)
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            var serializer = new SoapEnvelopeSerializer();

            const string contentType = 
                "multipart/related; boundary=\"=-dXYE+NJdacou7AbmYZgUPw==\"; type=\"application/soap+xml\"; charset=\"utf-8\"";

            AS4Message as4Message = 
                await serializer.DeserializeAsync(memoryStream, contentType, CancellationToken.None);

            return new MessagingContext(as4Message, MessagingContextMode.Unknown);
        }

        protected ReceivingProcessingMode ReceivingPModeWithAllowedSigningVerification()
        {
            var receivingPMode = new ReceivingProcessingMode();
            receivingPMode.Security.SigningVerification.Signature = Limit.Allowed;

            return receivingPMode;
        }

        private async Task<StepResult> ExerciseVerify(MessagingContext ctx)
        {
            var sut = new VerifySignatureAS4MessageStep(
                GetDataStoreContext,
                StubConfig.Default,
                new AS4MessageBodyFileStore(SerializerProvider.Default));

            return await sut.ExecuteAsync(ctx);
        }
    }
}