using System.Security.Cryptography.Xml;
using System.Xml;
using Eu.EDelivery.AS4.Security.Encryption;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.Encryption
{
    /// <summary>
    /// Testing <see cref="AS4EncryptedKey"/>
    /// </summary>
    public class GivenAS4EncryptedKeyFacts
    {
        public class GivenValidArguments : GivenAS4EncryptedKeyFacts
        {
            [Theory, InlineData("reference-id")]
            public void ThenGetReferenceIdSucceeds(string id)
            {
                // Arrange
                var encryptedKey = new EncryptedKey { Id = id };

                var as4EncryptedKey = new AS4EncryptedKey(encryptedKey);

                // Act
                string referenceId = as4EncryptedKey.GetReferenceId();
                // Assert
                Assert.Equal(id, referenceId);
            }

            [Fact]
            public void ThenGetCipherDataSucceeds()
            {
                // Arrange
                var cipherData = new CipherData { CipherValue = new byte[] { 20 } };
                var encryptedKey = new EncryptedKey { CipherData = cipherData };

                var as4EncryptedKey = new AS4EncryptedKey(encryptedKey);

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
                var as4EncryptedKey = AS4EncryptedKey.LoadFromXmlDocument(xmlDocument);

                // Assert
                Assert.Equal("EK-501d4b2b-5d8459ed-c0c0-45a5-a0c4-4bde7cf06a38", as4EncryptedKey.GetReferenceId());
            }

            [Fact]
            public void ThenAppendEncryptedKeySucceeds()
            {
                // Arrange
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(Properties.Resources.as4_encrypted_envelope);
                var as4EncryptedKey = AS4EncryptedKey.LoadFromXmlDocument(xmlDocument);

                xmlDocument = new XmlDocument();
                XmlElement securityElement = xmlDocument
                    .CreateElement("wsse", "Security", Constants.Namespaces.WssSecuritySecExt);
                // Act
                as4EncryptedKey.AppendEncryptedKey(securityElement);
                // Assert
                Assert.Equal("EncryptedKey", securityElement.FirstChild.LocalName);
            }
        }
    }
}