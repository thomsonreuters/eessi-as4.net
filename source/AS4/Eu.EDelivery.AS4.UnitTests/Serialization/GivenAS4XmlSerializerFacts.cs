using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Serialization
{
    /// <summary>
    /// Testing <see cref="AS4XmlSerializer"/>
    /// </summary>
    public class GivenAS4XmlSerializerfacts
    {
        public class ToXmlString
        {
            [Fact]
            public async Task AsyncSerializeToValidXml()
            {
                // Arrange
                var deliverMessage = new DeliverMessage();

                // Act
                string actualXml = await AS4XmlSerializer.ToStringAsync(deliverMessage);

                // Assert
                Assert.NotEmpty(actualXml);
            }
        }

        public class Serialize
        {
            [Fact]
            public async Task SendingPMode()
            {
                // Arrange
                var expectedPMode = new SendingProcessingMode { Id = "expected-id" };

                // Act
                Stream actualPModeStream = await AS4XmlSerializer.ToStreamAsync(expectedPMode);

                // Assert
                SendingProcessingMode actualPMode = DeserializeExpectedPMode(actualPModeStream);
                Assert.Equal(expectedPMode.Id, actualPMode.Id);
            }

            [Fact]
            public async Task SendingPModeWithEncryptionFindCriteria()
            {
                // Arrange
                var expectedPMode = new SendingProcessingMode { Id = "expected-id" };
                expectedPMode.Security.Encryption.CertificateType = CertificateChoiceType.FindCertificate;
                expectedPMode.Security.Encryption.EncryptionCertificateInformation = new CertificateFindCriteria()
                {
                    CertificateFindType = X509FindType.FindByCertificatePolicy,
                    CertificateFindValue = "SomeValue"
                };

                // Act
                Stream actualPModeStream = await AS4XmlSerializer.ToStreamAsync(expectedPMode);

                // Assert
                SendingProcessingMode actualPMode = DeserializeExpectedPMode(actualPModeStream);
                Assert.Equal(expectedPMode.Id, actualPMode.Id);
                Assert.Equal(expectedPMode.Security.Encryption.CertificateType, actualPMode.Security.Encryption.CertificateType);

                var expectedPublicKeyCriteria = (CertificateFindCriteria)expectedPMode.Security.Encryption.EncryptionCertificateInformation;
                var actualPublicKeyCriteria = (CertificateFindCriteria)actualPMode.Security.Encryption.EncryptionCertificateInformation;

                Assert.Equal(expectedPublicKeyCriteria.CertificateFindType, actualPublicKeyCriteria.CertificateFindType);
                Assert.Equal(expectedPublicKeyCriteria.CertificateFindValue, actualPublicKeyCriteria.CertificateFindValue);
            }

            [Fact]
            public async Task SendingPModeWithEncryptionCertificate()
            {
                // Arrange
                var expectedPMode = new SendingProcessingMode { Id = "expected-id" };
                expectedPMode.Security.Encryption.CertificateType = CertificateChoiceType.EmbeddedCertificate;
                expectedPMode.Security.Encryption.EncryptionCertificateInformation = new PublicKeyCertificate()
                {
                    Certificate = "ABCDEFGH"
                };

                // Act
                Stream actualPModeStream = await AS4XmlSerializer.ToStreamAsync(expectedPMode);

                // Assert
                SendingProcessingMode actualPMode = DeserializeExpectedPMode(actualPModeStream);
                Assert.Equal(expectedPMode.Id, actualPMode.Id);
                Assert.Equal(expectedPMode.Security.Encryption.CertificateType, CertificateChoiceType.EmbeddedCertificate);
                Assert.Equal("ABCDEFGH", ((PublicKeyCertificate)actualPMode.Security.Encryption.EncryptionCertificateInformation).Certificate);
            }

            private static SendingProcessingMode DeserializeExpectedPMode(Stream actualPModeStream)
            {
                using (var memoryStream = new MemoryStream())
                {
                    actualPModeStream.CopyTo(memoryStream);
                    memoryStream.Position = 0;

                    using (var streamReader = new StringReader(Encoding.UTF8.GetString(memoryStream.ToArray())))
                    using (XmlReader reader = XmlReader.Create(streamReader))
                    {
                        var xmlSerializer = new XmlSerializer(typeof(SendingProcessingMode));
                        return xmlSerializer.Deserialize(reader) as SendingProcessingMode;
                    }
                }
            }
        }

        public class Deserialize
        {
            [Fact]
            public async Task FilledWithPModeData()
            {
                // Arrange
                var expectedPMode = new SendingProcessingMode();
                using (Stream pmodeStream = SerializeExpectedPMode(expectedPMode))
                {
                    // Act
                    var actualPMode = await AS4XmlSerializer.FromStreamAsync<SendingProcessingMode>(pmodeStream);

                    // Assert
                    Assert.Equal(expectedPMode.Id, actualPMode.Id);
                }
            }

            private static Stream SerializeExpectedPMode(SendingProcessingMode expectedPMode)
            {
                var pmodeStream = new MemoryStream();
                var xmlSerializer = new XmlSerializer(typeof(SendingProcessingMode));
                xmlSerializer.Serialize(pmodeStream, expectedPMode);

                return pmodeStream;
            }
        }
    }
}