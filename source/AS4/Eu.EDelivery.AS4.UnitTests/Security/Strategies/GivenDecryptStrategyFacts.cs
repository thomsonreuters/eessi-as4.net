using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.Strategies
{
    public class GivenDecryptStrategyFacts
    {
        [Fact]
        public async void ThenDecryptDecryptsTheAttachmentsCorrectly()
        {
            // Arrange
            AS4Message as4Message = await GetEncryptedMessageAsync();
            X509Certificate2 decryptCertificate = CreateDecryptCertificate();

            // Act
            as4Message.Decrypt(decryptCertificate);

            // Assert
            Assert.Equal(Properties.Resources.flower1, GetAttachmentContents(as4Message.Attachments.ElementAt(0)));
            Assert.Equal(Properties.Resources.flower2, GetAttachmentContents(as4Message.Attachments.ElementAt(1)));
        }

        [Fact]
        public async Task ThenDecryptThrowsAnAS4Exception()
        {
            // Arrange
            AS4Message as4Message = await GetEncryptedMessageAsync();
            var certificate = new StubCertificateRepository().GetStubCertificate();

            // Act&Assert
            Assert.ThrowsAny<Exception>(() => as4Message.Decrypt(certificate));
        }

        private static async Task<AS4Message> GetEncryptedMessageAsync()
        {
            Stream inputStream = new MemoryStream(Properties.Resources.as4_encrypted_message);
            var serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());

            var message = await serializer.DeserializeAsync(
                inputStream,
                "multipart/related; boundary=\"MIMEBoundary_64ed729f813b10a65dfdc363e469e2206ff40c4aa5f4bd11\"",
                CancellationToken.None);

            Assert.True(message.IsEncrypted, "The AS4 Message to use in this testcase should be encrypted");

            return message;
        }

        private static X509Certificate2 CreateDecryptCertificate()
        {
            return new X509Certificate2(
                Properties.Resources.holodeck_partyc_certificate,
                "ExampleC",
                X509KeyStorageFlags.Exportable);
        }

        private static byte[] GetAttachmentContents(Attachment attachment)
        {
            var attachmentInMemory = new MemoryStream();
            attachment.Content.CopyTo(attachmentInMemory);

            return attachmentInMemory.ToArray();
        }
    }
}