using System.Xml;
using Eu.EDelivery.AS4.Security.Algorithms;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.Algorithms
{
    /// <summary>
    /// Testing <see cref="SignatureAlgorithmProvider" />
    /// </summary>
    public class GivenSignatureAlgorithmProviderFacts
    {
        private readonly SignatureAlgorithmProvider _provider;

        public GivenSignatureAlgorithmProviderFacts()
        {
            _provider = new SignatureAlgorithmProvider();
        }

        public class GivenValidArguments : GivenSignatureAlgorithmProviderFacts
        {
            [Fact]
            public void ThenGetRsaSha256SignatureAlgorithmFromProviderSucceedsForNamespace()
            {
                // Arrange
                const string key = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

                // Act
                SignatureAlgorithm signatureAlgorithm = _provider.Get(key);

                // Assert
                Assert.NotNull(signatureAlgorithm);
                Assert.IsType<RsaPkCs1Sha256SignatureAlgorithm>(signatureAlgorithm);
            }

            [Fact]
            public void ThenGetRsaSha256SignatureAlgorithmFromProviderSucceedsForXmlDocument()
            {
                // Arrange
                const string key = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
                XmlDocument xmlDocument = GetEnvelopeDocument(key);

                // Act
                SignatureAlgorithm signatureAlgorithm = _provider.Get(xmlDocument);

                // Assert
                Assert.NotNull(signatureAlgorithm);
                Assert.IsType<RsaPkCs1Sha256SignatureAlgorithm>(signatureAlgorithm);
            }

            [Fact]
            public void ThenGetRsaSha384SignatureAlgorithmFromProviderSucceedsForNamespace()
            {
                // Arrange
                const string key = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha384";

                // Act
                SignatureAlgorithm signatureAlgorithm = _provider.Get(key);

                // Assert
                Assert.NotNull(signatureAlgorithm);
                Assert.IsType<RsaPkCs1Sha384SignatureDescription>(signatureAlgorithm);
            }

            [Fact]
            public void ThenGetRsaSha384SignatureAlgorithmFromProviderSucceedsForXmlDocument()
            {
                // Arrange
                const string key = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha384";
                XmlDocument xmlDocument = GetEnvelopeDocument(key);

                // Act
                SignatureAlgorithm signatureAlgorithm = _provider.Get(xmlDocument);

                // Assert
                Assert.NotNull(signatureAlgorithm);
                Assert.IsType<RsaPkCs1Sha384SignatureDescription>(signatureAlgorithm);
            }

            [Fact]
            public void ThenGetRsaSha512SignatureAlgorithmFromProviderSucceedsForNamespace()
            {
                // Arrange
                const string key = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512";

                // Act
                SignatureAlgorithm signatureAlgorithm = _provider.Get(key);

                // Assert
                Assert.NotNull(signatureAlgorithm);
                Assert.IsType<RsaPkCs1Sha512SignatureAlgorithm>(signatureAlgorithm);
            }

            [Fact]
            public void ThenGetRsaSha512SignatureAlgorithmFromProviderSucceedsForXmlDocument()
            {
                // Arrange
                const string key = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512";
                XmlDocument xmlDocument = GetEnvelopeDocument(key);

                // Act
                SignatureAlgorithm signatureAlgorithm = _provider.Get(xmlDocument);

                // Assert
                Assert.NotNull(signatureAlgorithm);
                Assert.IsType<RsaPkCs1Sha512SignatureAlgorithm>(signatureAlgorithm);
            }
        }

        protected XmlDocument GetEnvelopeDocument(string algorithm)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(
                $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><SignatureMethod Algorithm=\"{algorithm}\"/>");

            return xmlDocument;
        }
    }
}