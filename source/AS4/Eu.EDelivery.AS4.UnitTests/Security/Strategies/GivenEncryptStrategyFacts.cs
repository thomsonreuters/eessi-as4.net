using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Eu.EDelivery.AS4.Builders.Security;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Security.Strategies;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.Strategies
{
    public class GivenEncryptStrategyFacts
    {
        [Fact]
        public void ThenEncryptEncryptsTheAttachmentsCorrectly()
        {
            // Arrange
            var as4Message = CreateAS4Message();
            long originalAttachmentLength = as4Message.Attachments.First().Content.Length;

            IEncryptionStrategy encryptionStrategy = EncryptionStrategyFor(as4Message, CreateEncryptionCertificate(),
                                                                           DataEncryptionConfiguration.Default);

            // Act
            encryptionStrategy.EncryptMessage();

            // Assert
            Attachment firstAttachment = as4Message.Attachments.ElementAt(0);
            Assert.NotEqual(originalAttachmentLength, firstAttachment?.Content.Length);
        }

        [Fact]
        public void FailsToEncrypt_IfInvalidKeySize()
        {
            // Arrange
            AS4Message as4Message = CreateAS4Message();

            EncryptionStrategy sut = EncryptionStrategyFor(as4Message, CreateEncryptionCertificate(),
                                                           new DataEncryptionConfiguration(AS4.Model.PMode.Encryption.Default.Algorithm, -1));

            // Act / Assert
            Assert.ThrowsAny<Exception>(() => sut.EncryptMessage());
        }

        private static AS4Message CreateAS4Message()
        {
            byte[] attachmentContents = Encoding.UTF8.GetBytes("hi!");
            var attachment = new Attachment("attachment-id") { Content = new MemoryStream(attachmentContents) };

            AS4Message as4Message = AS4Message.Create(pmode: null);
            as4Message.AddAttachment(attachment);

            return as4Message;
        }

        private static EncryptionStrategy EncryptionStrategyFor(AS4Message as4Message, X509Certificate2 encryptionCertificate, DataEncryptionConfiguration configuration) // SendingProcessingMode pmode)
        {
            return EncryptionStrategyBuilder
                .Create(as4Message, new KeyEncryptionConfiguration(encryptionCertificate))
                .WithDataEncryptionConfiguration(configuration)
                .Build();
        }

        private static X509Certificate2 CreateEncryptionCertificate()
        {
            // TODO: we should just have a public key certificate here, without the need to specify the password.
            return new X509Certificate2(
                Properties.Resources.holodeck_partyc_certificate,
                "ExampleC");
        }
    }
}