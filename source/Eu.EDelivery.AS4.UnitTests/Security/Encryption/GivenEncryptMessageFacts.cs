using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;
using static Eu.EDelivery.AS4.UnitTests.Properties.Resources;

namespace Eu.EDelivery.AS4.UnitTests.Security.Encryption
{
    public class GivenEncryptMessageFacts
    {
        [Fact]
        public void ThenEncryptEncryptsTheAttachmentsCorrectly()
        {
            // Arrange
            var as4Message = CreateAS4Message();
            long originalAttachmentLength = as4Message.Attachments.First().Content.Length;

            // Act
            as4Message.Encrypt(new KeyEncryptionConfiguration(CreateEncryptionCertificate()),
                               DataEncryptionConfiguration.Default);


            // Assert
            Assert.True(as4Message.IsEncrypted);

            Attachment firstAttachment = as4Message.Attachments.ElementAt(0);
            Assert.NotEqual(originalAttachmentLength, firstAttachment?.Content.Length);
        }

        [Fact]
        public void FailsToEncrypt_IfInvalidKeySize()
        {
            // Arrange
            AS4Message as4Message = CreateAS4Message();

            var keyEncryptionConfig = new KeyEncryptionConfiguration(CreateEncryptionCertificate());
            var dataEncryptionConfig = new DataEncryptionConfiguration(AS4.Model.PMode.Encryption.Default.Algorithm, -1);

            // Act / Assert
            Assert.ThrowsAny<Exception>(() => as4Message.Encrypt(keyEncryptionConfig, dataEncryptionConfig));
        }

        private static AS4Message CreateAS4Message()
        {
            byte[] attachmentContents = Encoding.UTF8.GetBytes("hi!");
            var attachment = new Attachment("attachment-id", new MemoryStream(attachmentContents), "text/plain");

            AS4Message as4Message = AS4Message.Create(pmode: null);
            as4Message.AddAttachment(attachment);

            return as4Message;
        }

        private static X509Certificate2 CreateEncryptionCertificate()
        {
            // TODO: we should just have a public key certificate here, without the need to specify the password.
            return new X509Certificate2(
                holodeck_partyc_certificate,
                "ExampleC");
        } 
    }
}