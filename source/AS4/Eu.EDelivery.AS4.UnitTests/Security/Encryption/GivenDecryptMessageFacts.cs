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
using static Eu.EDelivery.AS4.UnitTests.Properties.Resources;

namespace Eu.EDelivery.AS4.UnitTests.Security.Encryption
{
    public class GivenDecryptMessageFacts
    {
        [Fact]
        public async Task Decrypt_Multiple_Image_Payloads_Correctly()
        {
            // Arrange
            AS4Message as4Message = await GetEncryptedMessageAsync();
            X509Certificate2 decryptCertificate = GetDecryptCertificate();

            // Act
            as4Message.Decrypt(decryptCertificate);

            // Assert
            Assert.Equal(
                new [] { flower1, flower2 },
                as4Message.Attachments.Select(GetAttachmentContents));
            Assert.All(
                as4Message.Attachments, 
                a => Assert.Equal("image/jpeg", a.ContentType));
        }

        [Fact]
        public async Task Decrypt_Sets_Compressed_ContentType_For_Payloads_Afterwards()
        {
            // Arrange
            AS4Message as4Message = await GetEncryptedCompressedMessageAsync();
            X509Certificate2 decryptCert = GetDecryptCertificate();

            // Act
            as4Message.Decrypt(decryptCert);

            // Assert
            Assert.All(
                as4Message.Attachments, 
                a => Assert.Equal("application/gzip", a.ContentType));
        }

        [Fact]
        public async Task Decrypt_Unmarks_SecurityHeader_As_Encrypted()
        {
            // Arrange
            AS4Message as4Message = await GetEncryptedMessageAsync();
            X509Certificate2 decryptCertificate = GetDecryptCertificate();

            Assert.True(as4Message.SecurityHeader.IsEncrypted);

            // Act
            as4Message.Decrypt(decryptCertificate);

            Assert.False(as4Message.SecurityHeader.IsEncrypted);
        }

        [Fact]
        public async Task Decrypt_Removes_Encryption_Elements_From_SecurityHeader()
        {
            // Arrange
            AS4Message as4Message = await GetEncryptedMessageAsync();
            X509Certificate2 decryptCertificate = GetDecryptCertificate();

            // Act
            as4Message.Decrypt(decryptCertificate);

            // Assert
            var encryptedKeyNode = as4Message.SecurityHeader.GetXml().SelectSingleNode("//*[local-name()='EncryptedKey']");
            Assert.Null(encryptedKeyNode);

            var encryptedDatas = as4Message.SecurityHeader.GetXml().SelectNodes("//*[local-name()='EncryptedData']");
            Assert.True(encryptedDatas == null || encryptedDatas.Count == 0);
        }

        [Fact]
        public async Task Decrypt_Fails_When_Wrong_Decryption_Certificate_Is_Given()
        {
            // Arrange
            AS4Message as4Message = await GetEncryptedMessageAsync();
            var certificate = new StubCertificateRepository().GetStubCertificate();

            // Act&Assert
            Assert.ThrowsAny<Exception>(() => as4Message.Decrypt(certificate));
        }

        private static Task<AS4Message> GetEncryptedMessageAsync()
        {
            return DeserializeEncryptedMessageAsync(
                as4_encrypted_message,
                "multipart/related; boundary=\"MIMEBoundary_64ed729f813b10a65dfdc363e469e2206ff40c4aa5f4bd11\"");
        }

        private static Task<AS4Message> GetEncryptedCompressedMessageAsync()
        {
            return DeserializeEncryptedMessageAsync(
                as4_encrypted_compressed_message,
                "multipart/related; boundary=\"=-6sJmyirLVoAPyUJUzCWk0w==\"");
        }

        private static async Task<AS4Message> DeserializeEncryptedMessageAsync(byte[] contents, string contentType)
        {
            var inputStr = new MemoryStream(contents);

            AS4Message output =
                await SerializerProvider
                      .Default
                      .Get(contentType)
                      .DeserializeAsync(inputStr, contentType, CancellationToken.None);

            Assert.True(output.IsEncrypted, "The AS4Message to use in this testcase should be encrypted");
            Assert.All(output.Attachments, a => Assert.Equal("application/octet-stream", a.ContentType));

            return output;
        }

        private static X509Certificate2 GetDecryptCertificate()
        {
            return new X509Certificate2(
                holodeck_partyc_certificate,
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