using System.Collections.Generic;
using System.IO;
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

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing <see cref="DecryptAS4MessageStep" />
    /// </summary>
    public class GivenDecryptAS4MessageStepFacts
    {        
        private readonly IStep _step;

        public GivenDecryptAS4MessageStepFacts()
        {
            var mockedRespository = new Mock<ICertificateRepository>();

            mockedRespository.Setup(r => r.GetCertificate(It.IsAny<X509FindType>(), It.IsAny<string>()))
                             .Returns(new X509Certificate2(Properties.Resources.holodeck_partyc_certificate, "ExampleC", X509KeyStorageFlags.Exportable));

            _step = new DecryptAS4MessageStep(mockedRespository.Object);
        }

        public class GivenValidArguments : GivenDecryptAS4MessageStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepSucceedsAsync()
            {
                // Arrange
                AS4Message as4Message = await GetEncryptedAS4MessageAsync();
                var context = new MessagingContext(as4Message, MessagingContextMode.Receive)
                {
                    ReceivingPMode = new ReceivingProcessingMode()
                };
                context.ReceivingPMode.Security.Decryption.Encryption = Limit.Allowed;
                context.ReceivingPMode.Security.Decryption.DecryptCertificateInformation =
                    new CertificateFindCriteria
                    {
                        CertificateFindValue = "",
                        CertificateFindType = X509FindType.FindByIssuerName
                    };

                // Act
                StepResult stepResult = await _step.ExecuteAsync(context, CancellationToken.None);

                // Assert
                Assert.True(stepResult.MessagingContext.AS4Message.IsEncrypted);
            }

            [Fact]
            public async Task TestIfAttachmentContentTypeIsSetBackToOriginal()
            {
                // Arrange
                AS4Message as4Message = await GetEncryptedAS4MessageAsync();
                var context = new MessagingContext(as4Message, MessagingContextMode.Receive) { ReceivingPMode = new ReceivingProcessingMode() };
                context.ReceivingPMode.Security.Decryption.Encryption = Limit.Allowed;
                context.ReceivingPMode.Security.Decryption.DecryptCertificateInformation =
                    new CertificateFindCriteria()
                    {
                        CertificateFindType = X509FindType.FindBySerialNumber
                    };

                // Act
                StepResult result = await _step.ExecuteAsync(context, CancellationToken.None);

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
                var as4Message = await CreateEncryptedAS4Message();

                var internalMessage = new MessagingContext(as4Message, MessagingContextMode.Receive) { ReceivingPMode = new ReceivingProcessingMode() };
                internalMessage.ReceivingPMode.Security.Decryption.Encryption = Limit.NotAllowed;

                // Act
                StepResult result = await _step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                Assert.False(result.Succeeded);

                ErrorResult error = result.MessagingContext.ErrorResult;
                Assert.Equal(ErrorCode.Ebms0103, error.Code);
            }

            private static async Task<AS4Message> CreateEncryptedAS4Message()
            {
                var message = AS4Message.Create(new UserMessage("somemessage"));
                message.AddAttachment(new Attachment("some-attachment")
                {
                    Content =  Stream.Null
                });

                var encryptedMessage = AS4MessageUtils.EncryptWithCertificate(message, new StubCertificateRepository().GetStubCertificate());

                return await AS4MessageUtils.SerializeDeserializeAsync(encryptedMessage);
            }

            [Fact]
            public async Task ThenExecuteStepFailsWithRequiredEncryptionAsync()
            {
                // Arrange
                var context = new MessagingContext(AS4Message.Empty, MessagingContextMode.Receive) { ReceivingPMode = new ReceivingProcessingMode() };

                context.ReceivingPMode.Security.Decryption.Encryption = Limit.Required;

                // Act
                StepResult result = await _step.ExecuteAsync(context, CancellationToken.None);

                // Assert
                ErrorResult error = result.MessagingContext.ErrorResult;
                Assert.Equal(ErrorCode.Ebms0103, error.Code);
            }
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
            Stream inputStream = new MemoryStream(Properties.Resources.as4_encrypted_message);
            var serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());

            return serializer.DeserializeAsync(
                inputStream,
                "multipart/related; boundary=\"MIMEBoundary_64ed729f813b10a65dfdc363e469e2206ff40c4aa5f4bd11\"",
                CancellationToken.None);
        }
    }
}