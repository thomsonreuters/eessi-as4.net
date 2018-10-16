using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.TestUtils;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Moq;
using Xunit;
using static Eu.EDelivery.AS4.UnitTests.Properties.Resources;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing <see cref="DecryptAS4MessageStep" />
    /// </summary>
    public class GivenDecryptAS4MessageStepFacts
    {
        public class GivenValidArguments : GivenDecryptAS4MessageStepFacts
        {
            [Fact]
            public async Task Decrypt_Bundeld_Message_Correctly()
            {
                // Arrange
                AS4Message as4Message = await GetBundledEncryptedMessageAsync();

                // Act
                StepResult result = await ExerciseDecryption(as4Message);

                // Assert
                Assert.False(result.MessagingContext.AS4Message.IsEncrypted);
            }

            private static async Task<AS4Message> GetBundledEncryptedMessageAsync()
            {
                AS4Message bundled = await DeserializeToEncryptedMessage(
                    as4_bundled_encrypted_message,
                    "multipart/related; boundary=\"MIMEBoundary_64ed729f813b10a65dfdc363e469e2206ff40c4aa5f4bd11\"");

                Assert.True(
                    bundled.MessageUnits.Count() > 1, 
                    "Encrypted AS4Message was expected to be bundled (more than a single MessageUnit)");

                return bundled;
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsAsync()
            {
                // Arrange
                AS4Message as4Message = await GetEncryptedAS4MessageAsync();
                var context = new MessagingContext(as4Message, MessagingContextMode.Receive)
                {
                    ReceivingPMode = new ReceivingProcessingMode
                    {
                        Security = {Decryption =
                        {
                            Encryption = Limit.Allowed,
                            DecryptCertificateInformation = new CertificateFindCriteria
                            {
                                CertificateFindValue = "",
                                CertificateFindType = X509FindType.FindByIssuerName
                            }
                        }}
                    }
                };

                // Act
                StepResult stepResult = await ExerciseDecryption(context);

                // Assert
                Assert.False(stepResult.MessagingContext.AS4Message.IsEncrypted);
            }

            [Fact]
            public async Task TestIfAttachmentContentTypeIsSetBackToOriginal()
            {
                // Arrange
                AS4Message as4Message = await GetEncryptedAS4MessageAsync();
                var context = new MessagingContext(as4Message, MessagingContextMode.Receive)
                {
                    ReceivingPMode = new ReceivingProcessingMode
                    {
                        Security = {Decryption =
                        {
                            Encryption = Limit.Allowed,
                            DecryptCertificateInformation = new CertificateFindCriteria {CertificateFindType =  X509FindType.FindBySerialNumber}
                        }}
                    }
                };

                // Act
                StepResult result = await ExerciseDecryption(context);

                // Assert
                IEnumerable<Attachment> attachments = result.MessagingContext.AS4Message.Attachments;
                Assert.All(attachments, a => Assert.Equal("image/jpeg", a.ContentType));
            }
        }

        public class GivenInvalidArguments : GivenDecryptAS4MessageStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepFailsWithNotAllowedEncryptionAsync()
            {
                // Arrange
                AS4Message as4Message = await CreateEncryptedAS4Message();
                var internalMessage = new MessagingContext(as4Message, MessagingContextMode.Receive)
                {
                    ReceivingPMode = new ReceivingProcessingMode
                    {
                        Security = {Decryption = {Encryption = Limit.NotAllowed}}
                    }
                };

                // Act
                StepResult result = await ExerciseDecryption(internalMessage);

                // Assert
                Assert.False(result.Succeeded);

                ErrorResult error = result.MessagingContext.ErrorResult;
                Assert.Equal(ErrorCode.Ebms0103, error.Code);
            }

            private static async Task<AS4Message> CreateEncryptedAS4Message()
            {
                AS4Message message = AS4Message.Create(new UserMessage("somemessage"));
                message.AddAttachment(
                    new Attachment(
                        "some-attachment",
                        Stream.Null,
                        "text/plain"));

                AS4Message encryptedMessage =
                    AS4MessageUtils.EncryptWithCertificate(
                        message, new StubCertificateRepository().GetStubCertificate());

                return await AS4MessageUtils.SerializeDeserializeAsync(encryptedMessage);
            }

            [Fact]
            public async Task ThenExecuteStepFailsWithRequiredEncryptionAsync()
            {
                // Arrange
                var context = new MessagingContext(AS4Message.Empty, MessagingContextMode.Receive)
                {
                    ReceivingPMode = new ReceivingProcessingMode
                    {
                        Security = {Decryption = {Encryption = Limit.Required}}
                    }
                };

                // Act
                StepResult result = await ExerciseDecryption(context);

                // Assert
                ErrorResult error = result.MessagingContext.ErrorResult;
                Assert.Equal(ErrorCode.Ebms0103, error.Code);
            }

            [Fact]
            public async Task Decrypt_Fails_When_Attachment_Isnt_Referenced_By_EncryptedData()
            {
                // Arrange
                AS4Message m =
                    await DeserializeToEncryptedMessage(
                        as4_soap_wrong_encrypted_no_encrypteddata_for_attachment,
                        "multipart/related; boundary=\"MIMEBoundary_64ed729f813b10a65dfdc363e469e2206ff40c4aa5f4bd11\"");

                // Act
                StepResult result = await ExerciseDecryption(
                    new MessagingContext(m, MessagingContextMode.Receive)
                    {
                        ReceivingPMode = ReceivingPModeForDecryption()
                    });

                // Assert
                Assert.False(result.CanProceed);
                Assert.Equal(ErrorAlias.FailedDecryption, result.MessagingContext.ErrorResult.Alias);
            }
        }

        private static Task<StepResult> ExerciseDecryption(MessagingContext ctx)
        {
            var mockedRespository = new Mock<ICertificateRepository>();

            mockedRespository
                .Setup(r => r.GetCertificate(It.IsAny<X509FindType>(), It.IsAny<string>()))
                .Returns(new X509Certificate2(
                    rawData: holodeck_partyc_certificate, 
                    password: "ExampleC", 
                    keyStorageFlags: X509KeyStorageFlags.Exportable));

            var sut = new DecryptAS4MessageStep(mockedRespository.Object);
            return sut.ExecuteAsync(ctx);
        }

        private static async Task<AS4Message> DeserializeToEncryptedMessage(byte[] messageContents, string contentType)
        {
            Stream inputStream = new MemoryStream(messageContents);
            var serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());

            var message = await serializer.DeserializeAsync(
                inputStream,
                contentType);

            Assert.True(message.IsEncrypted, "The AS4 Message to use in this testcase should be encrypted");

            return message;
        }

        private static Task<StepResult> ExerciseDecryption(AS4Message msg)
        {
            var mockedRespository = new Mock<ICertificateRepository>();

            mockedRespository
                .Setup(r => r.GetCertificate(It.IsAny<X509FindType>(), It.IsAny<string>()))
                .Returns(new X509Certificate2(
                             rawData: holodeck_partyc_certificate,
                             password: "ExampleC",
                             keyStorageFlags: X509KeyStorageFlags.Exportable));

            var sut = new DecryptAS4MessageStep(mockedRespository.Object);
            return sut.ExecuteAsync(
                new MessagingContext(msg, MessagingContextMode.Receive)
                {
                    ReceivingPMode = ReceivingPModeForDecryption()
                });
        }

        private static ReceivingProcessingMode ReceivingPModeForDecryption()
        {
            return new ReceivingProcessingMode
            {
                Security =
                {
                    Decryption =
                    {
                        Encryption = Limit.Required,
                        CertificateType = PrivateKeyCertificateChoiceType.PrivateKeyCertificate,
                        DecryptCertificateInformation = new CertificateFindCriteria
                        {
                            CertificateFindType = X509FindType.FindBySubjectName,
                            CertificateFindValue = "ExampleC"
                        }
                    }
                }
            };
        }

        [Fact]
        public async Task TestEncryptedMessage_IfAttachmentsAreCorrectlyDeserialized()
        {
            // Act
            AS4Message sut = await GetEncryptedAS4MessageAsync();

            // Assert
            Assert.True(sut.HasAttachments, "Deserialized message hasn't got any attachments");
            Assert.All(sut.Attachments, a => Assert.Equal("application/octet-stream", a.ContentType));
        }

        protected Task<AS4Message> GetEncryptedAS4MessageAsync()
        {
            Stream inputStream = new MemoryStream(as4_encrypted_message);
            var serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());

            return serializer.DeserializeAsync(
                inputStream,
                "multipart/related; boundary=\"MIMEBoundary_64ed729f813b10a65dfdc363e469e2206ff40c4aa5f4bd11\"");
        }
    }
}