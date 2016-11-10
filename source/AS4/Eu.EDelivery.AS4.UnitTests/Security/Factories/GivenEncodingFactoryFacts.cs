using Eu.EDelivery.AS4.Security.Factories;
using Org.BouncyCastle.Crypto.Encodings;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.Factories
{
    /// <summary>
    /// Testing <see cref="EncodingFactory"/>
    /// </summary>
    public class GivenEncodingFactoryFacts
    {
        public class GivenValidArguments : GivenEncodingFactoryFacts
        {
            [Fact]
            public void ThenCreateEncodingWithDefaultsSucceeds()
            {
                // Act
                OaepEncoding encoding = EncodingFactory.Instance.Create();
                // Assert
                Assert.Equal("RSA/OAEPPadding", encoding.AlgorithmName);
            }

            [Fact]
            public void ThenCreateEncodingWithGivenAlgorithmSucceeds()
            {
                // Arrange
                const string algorithmName = "http://www.w3.org/2001/04/xmlenc#sha256";
                // Act
                OaepEncoding encoding = EncodingFactory.Instance.Create(algorithmName);
                // Assert
                Assert.Equal("RSA/OAEPPadding", encoding.AlgorithmName);
            }
        }
    }
}
