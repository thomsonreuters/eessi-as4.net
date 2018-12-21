using System.Linq;
using System.Xml;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.References
{
    public class GivenKeyIdentifierSecurityTokenReferenceFacts
    {
        [Fact]
        public void ThenGetXmlContainsKeyIdentifier()
        {
            // Arrange
            const string subjectKeyIdentifier = "hRmOyHw/oLIBBsGKp/L9qzCUZ1k=";
            var reference = new KeyIdentifierSecurityTokenReference(new StubCertificateRepository().GetStubCertificate());

            // Act
            XmlElement xml = reference.GetXml();

            // Assert
            Assert.NotNull(xml);
            Assert.Equal("wsse:SecurityTokenReference", xml.Name);
            Assert.Equal(Constants.Namespaces.WssSecuritySecExt, xml.NamespaceURI);

            XmlNode keyIdentifierNode = xml.SelectEbmsNode("/wsse:KeyIdentifier");
            Assert.Equal(subjectKeyIdentifier, keyIdentifierNode.InnerText);
            Assert.NotNull(keyIdentifierNode.Attributes);
            Assert.Collection(
                keyIdentifierNode.Attributes.Cast<XmlAttribute>(),
                a1 => a1.AssertEbmsAttribute("EncodingType", Constants.Namespaces.Base64Binary),
                a2 => a2.AssertEbmsAttribute("ValueType", Constants.Namespaces.SubjectKeyIdentifier));
        }

        [Fact]
        public void ThenLoadXmlGetsCertificateFromXml()
        {
            // Arrange
            var doc = new XmlDocument();
            doc.LoadXml(
                @"<wsse:SecurityTokenReference 
                        xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" 
                        xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd""
                        wsu:Id=""STR-501d4b2b-3cacedf4-f6a1-4c02-be23-b8763e037755"">
                    <wsse:KeyIdentifier 
                            EncodingType=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary""
                            ValueType=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509SubjectKeyIdentifier""
                            >Vdi1FeoKetwvEYlNqvb9qUPAins=</wsse:KeyIdentifier>
                </wsse:SecurityTokenReference>");

            // Arrange and Act
            var reference = new KeyIdentifierSecurityTokenReference(doc.DocumentElement, new StubCertificateRepository());

            Assert.NotNull(reference.Certificate);
            Assert.Equal("CN=AccessPointA", reference.Certificate.Subject);
        }
    }
}