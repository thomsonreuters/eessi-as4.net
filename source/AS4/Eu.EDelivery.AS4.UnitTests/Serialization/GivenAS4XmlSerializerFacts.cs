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
                expectedPMode.Security.Encryption.PublicKeyType = PublicKeyChoiceType.PublicKeyFindCriteria;
                expectedPMode.Security.Encryption.PublicKeyInformation = new PublicKeyFindCriteria()
                {
                    PublicKeyFindType = X509FindType.FindByCertificatePolicy,
                    PublicKeyFindValue = "SomeValue"
                };

                // Act
                Stream actualPModeStream = await AS4XmlSerializer.ToStreamAsync(expectedPMode);

                // Assert
                SendingProcessingMode actualPMode = DeserializeExpectedPMode(actualPModeStream);
                Assert.Equal(expectedPMode.Id, actualPMode.Id);
                Assert.Equal(expectedPMode.Security.Encryption.PublicKeyType, actualPMode.Security.Encryption.PublicKeyType);

                var expectedPublicKeyCriteria = (PublicKeyFindCriteria)expectedPMode.Security.Encryption.PublicKeyInformation;
                var actualPublicKeyCriteria = (PublicKeyFindCriteria)actualPMode.Security.Encryption.PublicKeyInformation;

                Assert.Equal(expectedPublicKeyCriteria.PublicKeyFindType, actualPublicKeyCriteria.PublicKeyFindType);
                Assert.Equal(expectedPublicKeyCriteria.PublicKeyFindValue, actualPublicKeyCriteria.PublicKeyFindValue);
            }

            [Fact]
            public async Task SendingPModeWithEncryptionCertificate()
            {
                // Arrange
                var expectedPMode = new SendingProcessingMode { Id = "expected-id" };
                expectedPMode.Security.Encryption.PublicKeyType = PublicKeyChoiceType.PublicKeyCertificate;
                expectedPMode.Security.Encryption.PublicKeyInformation = new PublicKeyCertificate()
                {
                    Certificate = "ABCDEFGH"
                };

                // Act
                Stream actualPModeStream = await AS4XmlSerializer.ToStreamAsync(expectedPMode);

                // Assert
                SendingProcessingMode actualPMode = DeserializeExpectedPMode(actualPModeStream);
                Assert.Equal(expectedPMode.Id, actualPMode.Id);
                Assert.Equal(expectedPMode.Security.Encryption.PublicKeyType, PublicKeyChoiceType.PublicKeyCertificate);
                Assert.Equal("ABCDEFGH", ((PublicKeyCertificate)actualPMode.Security.Encryption.PublicKeyInformation).Certificate);
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