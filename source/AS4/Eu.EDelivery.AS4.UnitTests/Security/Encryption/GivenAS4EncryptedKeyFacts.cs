using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Security.Strategies;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.Encryption
{
    /// <summary>
    /// Testing <see cref="AS4EncryptedKey" />
    /// </summary>
    public class GivenAS4EncryptedKeyFacts
    {
        public class GivenValidArguments : GivenAS4EncryptedKeyFacts
        {
            [Theory]
            [InlineData("reference-id")]
            public void ThenGetReferenceIdSucceeds(string id)
            {
                // Arrange
                var encryptedKey = new EncryptedKey {Id = id};

                AS4EncryptedKey as4EncryptedKey = AS4EncryptedKey.FromEncryptedKey(encryptedKey);

                // Act
                string referenceId = as4EncryptedKey.GetReferenceId();

                // Assert
                Assert.Equal(id, referenceId);
            }

            [Fact]
            public void ThenAppendEncryptedKeySucceeds()
            {
                // Arrange
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(Properties.Resources.as4_encrypted_envelope);
                AS4EncryptedKey as4EncryptedKey = AS4EncryptedKey.LoadFromXmlDocument(xmlDocument);

                xmlDocument = new XmlDocument();
                XmlElement securityElement = xmlDocument.CreateElement(
                    "wsse",
                    "Security",
                    Constants.Namespaces.WssSecuritySecExt);

                // Act
                as4EncryptedKey.AppendEncryptedKey(securityElement);

                // Assert
                Assert.Equal("EncryptedKey", securityElement.FirstChild.LocalName);
            }

            [Fact]
            public void ThenGetCipherDataSucceeds()
            {
                // Arrange
                var cipherData = new CipherData {CipherValue = new byte[] {20}};
                var encryptedKey = new EncryptedKey {CipherData = cipherData};

                AS4EncryptedKey as4EncryptedKey = AS4EncryptedKey.FromEncryptedKey(encryptedKey);

                // Act
                CipherData as4CipherData = as4EncryptedKey.GetCipherData();

                // Assert
                Assert.Equal(cipherData, as4CipherData);
            }

            [Fact]
            public void ThenLoadEncryptedKeySucceeds()
            {
                // Arrange
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(Properties.Resources.as4_encrypted_envelope);

                // Act
                AS4EncryptedKey as4EncryptedKey = AS4EncryptedKey.LoadFromXmlDocument(xmlDocument);

                // Assert
                Assert.Equal("EK-501d4b2b-5d8459ed-c0c0-45a5-a0c4-4bde7cf06a38", as4EncryptedKey.GetReferenceId());
            }
        }
    }

    public class AS4EncryptedKeyBuilderFacts
    {
        private static byte[] GenerateEncryptionKey()
        {
            byte[] encryptionKey;

            using (var generator = new RijndaelManaged {BlockSize = 256})
            {
                encryptionKey = generator.Key;
            }

            return encryptionKey;
        }

        private static X509Certificate2 GetCertificate()
        {
            return new X509Certificate2(
                Properties.Resources.holodeck_partyc_certificate,
                "ExampleC",
                X509KeyStorageFlags.Exportable);
        }

        public class GivenDefaultAlgorithms
        {
            [Fact]
            public void ThenCreateAS4EncryptedKeySucceeds()
            {
                byte[] encryptionKey = GenerateEncryptionKey();

                AS4EncryptedKey key =
                    AS4EncryptedKey.CreateEncryptedKeyBuilderForKey(encryptionKey, GetCertificate()).Build();

                Assert.Equal(EncryptionStrategy.XmlEncRSAOAEPUrlWithMgf, key.GetEncryptionAlgorithm());
                Assert.Equal(EncryptionStrategy.XmlEncSHA1Url, key.GetDigestAlgorithm());
            }
        }

        public class GivenSpecificAlgorithms
        {
            [Theory]
            [InlineData("http://www.w3.org/2009/xmlenc11#rsa-oaep", EncryptedXml.XmlEncSHA256Url, null)]
            [InlineData("http://www.w3.org/2009/xmlenc11#rsa-oaep", EncryptedXml.XmlEncSHA256Url,
                "http://www.w3.org/2009/xmlenc11#mgf1sha256")]
            public void ThenCreateAS4EncryptedKeySucceeds(string algorithm, string digest, string mgf)
            {
                byte[] encryptionKey = GenerateEncryptionKey();

                AS4EncryptedKey key =
                    AS4EncryptedKey.CreateEncryptedKeyBuilderForKey(encryptionKey, GetCertificate())
                                   .WithEncryptionMethod("http://www.w3.org/2009/xmlenc11#rsa-oaep")
                                   .WithDigest(EncryptedXml.XmlEncSHA256Url)
                                   .WithMgf(mgf)
                                   .Build();

                Assert.Equal(algorithm, key.GetEncryptionAlgorithm());
                Assert.Equal(digest, key.GetDigestAlgorithm());
                Assert.Equal(mgf, key.GetMaskGenerationFunction());
            }
        }
    }
}