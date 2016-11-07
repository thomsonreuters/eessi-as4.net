using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.References
{
    /// <summary>
    /// Testing the <see cref="KeyIdentifierSecurityTokenReference"/>
    /// </summary>
    public class GivenKeyIdentifierSecurityTokenReferenceFacts
    {
        private readonly KeyIdentifierSecurityTokenReference _reference;
        private readonly X509Certificate2 _dummyCertificate;

        public GivenKeyIdentifierSecurityTokenReferenceFacts()
        {
            this._dummyCertificate = new StubCertificateRepository().GetDummyCertificate();
            this._reference = new KeyIdentifierSecurityTokenReference {Certificate = this._dummyCertificate};
        }

        /// <summary>
        /// Testing if the Reference Succeeds for the "GetXml" Method
        /// </summary>
        public class GivenValidArgumentsForGetXml : GivenKeyIdentifierSecurityTokenReferenceFacts
        {
            [Fact]
            public void ThenGetXmlContainsSecurityTokenReference()
            {
                // Act
                XmlElement xmlElement = base._reference.GetXml();
                // Assert
                Assert.NotNull(xmlElement);
                Assert.Equal("wsse:SecurityTokenReference", xmlElement.Name);
                Assert.Equal(Constants.Namespaces.WssSecuritySecExt, xmlElement.NamespaceURI);
            }

            [Fact]
            public void ThenGetXmlContainsKeyIdentifier()
            {
                // Act
                XmlElement xmlElement = base._reference.GetXml();
                // Assert
                Assert.NotNull(xmlElement);
                Assert.Equal("wsse:KeyIdentifier", xmlElement.FirstChild.Name);
                Assert.Equal(Constants.Namespaces.WssSecuritySecExt, xmlElement.FirstChild.NamespaceURI);
            }

            [Fact]
            public void ThenGetXmlContainsKeyIdentifierEncodingTypeAttribute()
            {
                // Act
                XmlElement xmlElement = base._reference.GetXml();
                // Assert
                Assert.NotNull(xmlElement);
                Assert.NotNull(xmlElement.FirstChild.Attributes);
                Assert.Equal("EncodingType", xmlElement.FirstChild.Attributes[0].Name);
                Assert.Equal(Constants.Namespaces.Base64Binary, xmlElement.FirstChild.Attributes[0].Value);
            }

            [Fact]
            public void ThenGetXmlContainsKeyIdentifierValueTypeAttribute()
            {
                // Act
                XmlElement xmlElement = base._reference.GetXml();
                // Assert
                Assert.NotNull(xmlElement);
                Assert.NotNull(xmlElement.FirstChild.Attributes);
                Assert.Equal("ValueType", xmlElement.FirstChild.Attributes[1].Name);
                Assert.Equal(Constants.Namespaces.SubjectKeyIdentifier, xmlElement.FirstChild.Attributes[1].Value);
            }

            [Fact]
            public void ThenGetXmlContainsKeyIdentifierInnerText()
            {
                // Arrange
                const string subjectKeyIdentifier = "hRmOyHw/oLIBBsGKp/L9qzCUZ1k=";
                // Act
                XmlElement xmlElement = base._reference.GetXml();
                // Assert
                Assert.NotNull(xmlElement);
                Assert.Equal(subjectKeyIdentifier, xmlElement.FirstChild.InnerText);

            }
        }

        /// <summary>
        /// Testing if the Reference Succeeds for the "LoadXml" Method
        /// </summary>
        public class GivenValidArgumentsForLoadXml : GivenKeyIdentifierSecurityTokenReferenceFacts
        {
            [Fact]
            public void ThenLoadXmlGetsCertificateFromXml()
            {
                // Arrange
                base._reference.Certificate = base._dummyCertificate;
                XmlElement keyIdentifierXml = base._reference.GetXml();
                base._reference.Certificate = null;
                base._reference.CertificateRepository = new StubCertificateRepository();
                // Act
                base._reference.LoadXml(keyIdentifierXml);
                // Assert
                Assert.Equal(base._dummyCertificate, base._reference.Certificate);
            }
        }
    }
}
