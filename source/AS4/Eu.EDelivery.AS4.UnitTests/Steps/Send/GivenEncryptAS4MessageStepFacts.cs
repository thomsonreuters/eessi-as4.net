using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Common;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <see cref="EncryptAS4MessageStep" />
    /// </summary>
    public class GivenEncryptAS4MessageStepFacts
    {
        private EncryptAS4MessageStep _step;

        public GivenEncryptAS4MessageStepFacts()
        {
            var certificateRepositoryMock = new Mock<ICertificateRepository>();

            certificateRepositoryMock.Setup(x => x.GetCertificate(It.IsAny<X509FindType>(), It.IsAny<string>()))
                                     .Returns(new StubCertificateRepository().GetStubCertificate());

            _step = new EncryptAS4MessageStep(certificateRepositoryMock.Object);
        }

        public class GivenValidArguments : GivenEncryptAS4MessageStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepSucceedsAsync()
            {
                // Arrange
                AS4Message as4Message = CreateEncryptedAS4Message();

                // Act
                StepResult stepResult = await ExerciseEncryptAS4Message(as4Message);

                // Assert
                Assert.True(stepResult.InternalMessage.AS4Message.IsEncrypted);
            }

            [Fact]
            public async Task EncryptedDatasHasTakenOverAttachmentInfo()
            {
                // Arrange
                AS4Message as4Message = CreateEncryptedAS4Message();
                string expectedMimeType = as4Message.Attachments.First().ContentType;

                // Act
                StepResult result = await ExerciseEncryptAS4Message(as4Message);

                // Assert
                string actualMimeType = FirstEncryptedDataMimeTypeAttributeValue(result);

                Assert.Equal(expectedMimeType, actualMimeType);
                Assert.All(as4Message.Attachments, a => Assert.Equal("application/octet-stream", a.ContentType));
            }

            private async Task<StepResult> ExerciseEncryptAS4Message(AS4Message as4Message)
            {
                // Arrange
                var internalMessage = new InternalMessage(as4Message);

                // Act
                return await _step.ExecuteAsync(internalMessage, CancellationToken.None);
            }

            private static string FirstEncryptedDataMimeTypeAttributeValue(StepResult result)
            {
                XmlElement node = result.InternalMessage.AS4Message.SecurityHeader.GetXml();
                XmlNode attachmentNode = node.SelectSingleNode("//*[local-name()='EncryptedData']");

                return attachmentNode.Attributes.GetNamedItem("MimeType").Value;
            }
        }

        public class GivenInvalidArguments : GivenEncryptAS4MessageStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepFailsWithInvalidCertificateAsync()
            {
                // Arrange
                AS4Message as4Message = CreateEncryptedAS4Message();
                var internalMessage = new InternalMessage(as4Message);

                Mock<ICertificateRepository> certificateRepositoryMock = CreateFailedMockedCertificateRepository();
                _step = new EncryptAS4MessageStep(certificateRepositoryMock.Object);

                // Act / Assert
                await Assert.ThrowsAsync<AS4Exception>(
                    () => _step.ExecuteAsync(internalMessage, CancellationToken.None));
            }

            private Mock<ICertificateRepository> CreateFailedMockedCertificateRepository()
            {
                var certificateRepositoryMock = new Mock<ICertificateRepository>();

                certificateRepositoryMock.Setup(x => x.GetCertificate(It.IsAny<X509FindType>(), It.IsAny<string>()))
                                         .Throws(new Exception("Invalid Certificate"));

                return certificateRepositoryMock;
            }
        }

        protected AS4Message CreateEncryptedAS4Message()
        {
            Stream attachmentStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, encrypt me"));
            var attachment = new Attachment("attachment-id") {Content = attachmentStream, ContentType = "text/plain"};
            AS4Message as4Message = new AS4MessageBuilder().WithAttachment(attachment).Build();

            as4Message.SendingPMode = new SendingProcessingMode();
            as4Message.SendingPMode.Security.Encryption.IsEnabled = true;
            as4Message.SendingPMode.Security.Encryption.Algorithm = "http://www.w3.org/2009/xmlenc11#aes128-gcm";

            return as4Message;
        }
    }
}