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
            public void ThenRepositoryGetsCertificate()
            {
                // Arrange
                var binarySecurityTokenReference = new BinarySecurityTokenReference
                {
                    Certificate = new X509Certificate2()
                };
                var keyInfo = new KeyInfo();
                keyInfo.AddClause(binarySecurityTokenReference);

                var repository = new KeyInfoRepository(keyInfo);

                // Act
                X509Certificate2 certificate = repository.GetCertificate();

                // Assert
                Assert.NotNull(certificate);
                Assert.Equal(binarySecurityTokenReference.Certificate, certificate);
            }
        }

        public class GivenInvalidArguments : GivenKeyInfoRepositoryFacts
        {
            [Fact]
            public void ThenRepositoryFailsWhenGettingCertificate()
            {
                // Arrange
                var keyInfo = new KeyInfo();
                keyInfo.AddClause(new BinarySecurityTokenReference());
                var repository = new KeyInfoRepository(keyInfo);

                // Act
                X509Certificate2 certificate = repository.GetCertificate();

                // Assert
                Assert.Null(certificate);
            }
        }
    }
}