using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.Repositories
{
    /// <summary>
    /// Testing <see cref="KeyInfoRepository" />
    /// </summary>
    public class GivenKeyInfoRepositoryFacts
    {
        public class GivenValidArguments : GivenKeyInfoRepositoryFacts
        {
            [Fact]
            public void ThenRepositoryGetsCertificate_IfKeyInfoIsSecurityTokenReference()
            {
                // Arrange
                var expectedCertificate = new X509Certificate2();
                KeyInfo keyInfo = CreateKeyInfoWithSecurityTokenReference(expectedCertificate);

                var sut = new KeyInfoRepository(keyInfo);

                // Act
                X509Certificate2 actualCertificate = sut.GetCertificate();

                // Assert
                Assert.Equal(expectedCertificate, actualCertificate);
            }

            private static KeyInfo CreateKeyInfoWithSecurityTokenReference(X509Certificate2 expectedCertificate)
            {
                var binarySecurityTokenReference = new BinarySecurityTokenReference
                {
                    Certificate = expectedCertificate
                };

                var keyInfo = new KeyInfo();
                keyInfo.AddClause(binarySecurityTokenReference);

                return keyInfo;
            }

            [Fact]
            public void ThenRepositoryGetsCertificate_IfKeyInfoHasEmbeddedCertificate()
            {
                // Arrange
                var expectedCertificate = new X509Certificate2();
                KeyInfo keyInfo = CreateKeyInfoWithEmbeddedCertificate(expectedCertificate);

                var sut = new KeyInfoRepository(keyInfo);

                // Act
                X509Certificate2 actualCertificate = sut.GetCertificate();

                // Assert
                Assert.Equal(expectedCertificate, actualCertificate);
            }

            private static KeyInfo CreateKeyInfoWithEmbeddedCertificate(X509Certificate expectedCertificate)
            {
                var keyInfoData = new KeyInfoX509Data(expectedCertificate);
                var keyInfo = new KeyInfo();
                keyInfo.AddClause(keyInfoData);

                return keyInfo;
            }
        }

        public class GivenInvalidArguments : GivenKeyInfoRepositoryFacts
        {
            [Fact]
            public void ThenRepositoryFailsToGetCertificate_IfSecurityTokenReferenceHasntAnyCertificate()
            {
                // Arrange
                var keyInfo = new KeyInfo();
                keyInfo.AddClause(new BinarySecurityTokenReference());
                var repository = new KeyInfoRepository(keyInfo);

                // Act
                X509Certificate2 actualCertificate = repository.GetCertificate();

                // Assert
                Assert.Null(actualCertificate);
            }

            [Fact]
            public void ThenRepositoryFailsToGetCertificate_IfKeyInfoIsNull()
            {
                // Arrange
                var sut = new KeyInfoRepository(keyInfo: null);

                // Act
                X509Certificate2 actualCertificate = sut.GetCertificate();

                // Assert
                Assert.Null(actualCertificate);
            }

            [Fact]
            public void ThenRepositoryFailsToGetCertificate_IfKeyInfoHasntAnyClauses()
            {
                // Arrange
                var sut = new KeyInfoRepository(new KeyInfo());

                // Act
                X509Certificate2 actualCertificate = sut.GetCertificate();

                // Assert
                Assert.Null(actualCertificate);
            }
        }
    }
}