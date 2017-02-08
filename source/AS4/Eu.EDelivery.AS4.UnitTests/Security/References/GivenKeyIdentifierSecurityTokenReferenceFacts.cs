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

        public GivenKeyIdentifierSecurityTokenReferenceFacts()
        {
            var repository = new StubCertificateRepository();            
            this._reference = new KeyIdentifierSecurityTokenReference(repository);
            this._reference.Certificate = repository.GetDummyCertificate();
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
                XmlElement keyIdentifierXml = base._reference.GetXml();
                base._reference.Certificate = null;                
                // Act
                base._reference.LoadXml(keyIdentifierXml);
                // Assert
                Assert.Equal(new StubCertificateRepository().GetDummyCertificate(), base._reference.Certificate);
            }
        }
    }
}
