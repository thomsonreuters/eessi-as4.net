using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Builders.Security;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.Strategies
{
    public class GivenEncryptionStrategyFacts
    {
        public class GivenValidArgumentsForDecryptMessage : GivenEncryptionStrategyFacts
        {
            [Fact]
            public async void ThenDecryptDecryptsTheAttachmentsCorrectly()
            {
                // Arrange
                AS4Message as4Message = await GetEncryptedMessageAsync();
                X509Certificate2 decryptCertificate = CreateDecryptCertificate();

                EncryptionStrategy encryptionStrategy =
                    EncryptionStrategyBuilder.Create(as4Message.EnvelopeDocument)
                                             .WithCertificate(decryptCertificate)
                                             .WithAttachments(as4Message.Attachments)
                                             .Build();

                // Act
                encryptionStrategy.DecryptMessage();

                // Assert
                Assert.Equal(Properties.Resources.flower1, GetAttachmentContents(as4Message.Attachments.ElementAt(0)));
                Assert.Equal(Properties.Resources.flower2, GetAttachmentContents(as4Message.Attachments.ElementAt(1)));
            }

            [Fact]
            public void ThenEncryptEncryptsTheAttachmentsCorrectly()
            {
                // Arrange
                byte[] attachmentContents = Encoding.UTF8.GetBytes("hi!");
                var attachment = new Attachment("attachment-id") { Content = new MemoryStream(attachmentContents) };

                AS4Message as4Message = new AS4MessageBuilder().WithAttachment(attachment).Build();

                IEncryptionStrategy encryptionStrategy = CreateEncryptionStrategyForEncrypting(as4Message);

                // Act
                encryptionStrategy.EncryptMessage();

                // Assert
                Attachment firstAttachment = as4Message.Attachments.ElementAt(0);
                Assert.NotEqual(attachmentContents.Length, firstAttachment?.Content.Length);
            }

            private IEncryptionStrategy CreateEncryptionStrategyForEncrypting(AS4Message message)
            {
                XmlDocument xmlDocument = SerializeAS4Message(message);
                X509Certificate2 certificate = CreateDecryptCertificate();

                return EncryptionStrategyBuilder
                    .Create(xmlDocument)
                    .WithCertificate(certificate)
                    .WithAttachments(message.Attachments)
                    .Build();
            }
        }

        private static X509Certificate2 CreateDecryptCertificate()
        {
            return new X509Certificate2(
                Properties.Resources.holodeck_partyc_certificate,
                "ExampleC",
                X509KeyStorageFlags.Exportable);
        }

        public class GivenInvalidArgumentsForDecryptMessage : GivenEncryptionStrategyFacts
        {
            [Fact]
            public async Task ThenDecryptThrowsAnAS4Exception()
            {
                // Arrange
                AS4Message as4Message = await GetEncryptedMessageAsync();
                EncryptionStrategy encryptionStrategy =
                    EncryptionStrategyBuilder.Create(as4Message.EnvelopeDocument).Build();

                // Act&Assert
                Assert.ThrowsAny<Exception>(() => encryptionStrategy.DecryptMessage());
            }

            [Fact]
            public async Task FailsToDecrypt_IfInvalidKeySize()
            {
                // Arrange
                AS4Message as4Message = await GetEncryptedMessageAsync();
                KeyEncryption keyTransport = as4Message.SendingPMode.Security.Encryption.KeyTransport;
                keyTransport.KeySize = -1;

                EncryptionStrategy sut = EncryptionStrategyFor(as4Message, keyTransport);

                // Act / Assert
                Assert.ThrowsAny<Exception>(() => sut.EncryptMessage());
            }

            private static EncryptionStrategy EncryptionStrategyFor(AS4Message as4Message, KeyEncryption keyTransport)
            {
                var keyEncryptConfig = new KeyEncryptionConfiguration(tokenReference: null, keyEncryption: keyTransport);

                return EncryptionStrategyBuilder
                    .Create(as4Message.EnvelopeDocument)
                    .WithKeyEncryptionConfiguration(keyEncryptConfig)
                    .Build();
            }
        }

        protected XmlDocument SerializeAS4Message(AS4Message as4Message)
        {
            var memoryStream = new MemoryStream();
            var provider = new SerializerProvider();
            ISerializer serializer = provider.Get(Constants.ContentTypes.Soap);
            serializer.Serialize(as4Message, memoryStream, CancellationToken.None);

            return LoadEnvelopeToDocument(memoryStream);
        }

        private static XmlDocument LoadEnvelopeToDocument(Stream envelopeStream)
        {
            envelopeStream.Position = 0;
            var envelopeXmlDocument = new XmlDocument();
            var readerSettings = new XmlReaderSettings { CloseInput = false };

            using (XmlReader reader = XmlReader.Create(envelopeStream, readerSettings))
            {
                envelopeXmlDocument.Load(reader);
            }

            return envelopeXmlDocument;
        }

        protected static Task<AS4Message> GetEncryptedMessageAsync()
        {
            Stream inputStream = new MemoryStream(Properties.Resources.as4_encrypted_message);
            var serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());

            return serializer.DeserializeAsync(
                inputStream,
                "multipart/related; boundary=\"MIMEBoundary_64ed729f813b10a65dfdc363e469e2206ff40c4aa5f4bd11\"",
                CancellationToken.None);
        }

        protected byte[] GetAttachmentContents(Attachment attachment)
        {
            var attachmentInMemory = new MemoryStream();
            attachment.Content.CopyTo(attachmentInMemory);

            return attachmentInMemory.ToArray();
        }
    }
}