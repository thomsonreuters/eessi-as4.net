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
                SendingProcessingMode actualPMode = await ExerciseSerializeDeserialize(expectedPMode);

                // Assert
                Assert.Equal(expectedPMode.Id, actualPMode.Id);
            }

            [Fact]
            public async Task SendingPModeWithTlsConfiguration()
            {
                var expected = new ClientCertificateReference
                {
                    ClientCertificateFindType = X509FindType.FindBySubjectName,
                    ClientCertificateFindValue = "subject"
                };
                var before = new SendingProcessingMode
                {
                    PushConfiguration = new PushConfiguration
                    {
                        TlsConfiguration =
                        {
                            IsEnabled = true,
                            ClientCertificateInformation = expected
                        }
                    }
                };

                SendingProcessingMode after = await ExerciseSerializeDeserialize(before);

                var actual = after.PushConfiguration.TlsConfiguration.ClientCertificateInformation as ClientCertificateReference;

                Assert.NotNull(actual);
                Assert.Equal(expected.ClientCertificateFindType, actual.ClientCertificateFindType);
                Assert.Equal(expected.ClientCertificateFindValue, actual.ClientCertificateFindValue);
            }

            [Fact]
            public async Task SendingPModeWithEncryptionFindCriteria()
            {
                // Arrange
                var expectedPMode = new SendingProcessingMode { Id = "expected-id" };
                expectedPMode.Security.Encryption.CertificateType = PublicKeyCertificateChoiceType.CertificateFindCriteria;
                expectedPMode.Security.Encryption.EncryptionCertificateInformation = new CertificateFindCriteria()
                {
                    CertificateFindType = X509FindType.FindByCertificatePolicy,
                    CertificateFindValue = "SomeValue"
                };

                // Act
                SendingProcessingMode actualPMode = await ExerciseSerializeDeserialize(expectedPMode);

                // Assert
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
                expectedPMode.Security.Encryption.CertificateType = PublicKeyCertificateChoiceType.PublicKeyCertificate;
                expectedPMode.Security.Encryption.EncryptionCertificateInformation = new PublicKeyCertificate()
                {
                    Certificate = "ABCDEFGH"
                };

                // Act
                SendingProcessingMode actualPMode = await ExerciseSerializeDeserialize(expectedPMode);

                // Assert
                Assert.Equal(expectedPMode.Id, actualPMode.Id);
                Assert.Equal(PublicKeyCertificateChoiceType.PublicKeyCertificate, expectedPMode.Security.Encryption.CertificateType);
                Assert.Equal("ABCDEFGH", ((PublicKeyCertificate)actualPMode.Security.Encryption.EncryptionCertificateInformation).Certificate);
            }

            private static async Task<SendingProcessingMode> ExerciseSerializeDeserialize(SendingProcessingMode pmode)
            {
                using (Stream str = await AS4XmlSerializer.ToStreamAsync(pmode))
                using (var memoryStream = new MemoryStream())
                {
                    str.CopyTo(memoryStream);
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
            [Theory]
            [InlineData("<PartyInfo/>", false)]
            [InlineData("<PartyInfo><ToParty/></PartyInfo>", true)]
            public void SendingPMode_Party_Is_Not_Present_When_Non_Existing_Tag(string xml, bool specified)
            {
                var result = AS4XmlSerializer.FromString<PartyInfo>(xml);

                Assert.True((result.ToParty != null) == specified);
            }

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