using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.References
{
    public class GivenIssuerSecurityTokenReferenceFacts
    {
        [Fact]
        public void ThenGetXmlContainsSecurityTokenReference()
        {
            // Arrange
            var certRepository = new StubCertificateRepository();
            X509Certificate2 stubCertificate = certRepository.GetStubCertificate();

            // Act
            XmlElement xml = new IssuerSecurityTokenReference(stubCertificate).GetXml();

            // Assert
            Assert.NotNull(xml);
            Assert.Equal("wsse:SecurityTokenReference", xml.Name);
            Assert.Equal(Constants.Namespaces.WssSecuritySecExt, xml.NamespaceURI);

            XmlNode issuerName = xml.SelectEbmsNode("/dsig:X509Data/dsig:X509IssuerSerial/dsig:X509IssuerName");
            Assert.Equal(stubCertificate.IssuerName.Name, issuerName.InnerText);

            XmlNode serialNumberNode = xml.SelectEbmsNode("/dsig:X509Data/dsig:X509IssuerSerial/dsig:X509SerialNumber");
            string expectedSerialNumber = Convert.ToUInt64($"0x{stubCertificate.SerialNumber}", 16).ToString();
            Assert.Equal(expectedSerialNumber, serialNumberNode.InnerText);
        }

        [Fact]
        public void ThenLoadXmlGetsTheCertificateFromTheXml()
        {
            // Arrange
            var xmlDocument = new XmlDocument {PreserveWhitespace = true};
            xmlDocument.LoadXml(
                @"<wsse:SecurityTokenReference
                    xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" 
                    xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd""
                    xmlns:ds=""http://www.w3.org/2000/09/xmldsig#""
                    wsu:Id=""STR-501d4b2b-7385bec9-ef02-47a7-b2f4-b9c9c860195b"">
                        <ds:X509Data>
                            <ds:X509IssuerSerial>
                                <ds:X509IssuerName>CN=AccessPointA</ds:X509IssuerName>
                                <ds:X509SerialNumber>8207205864034169939</ds:X509SerialNumber>
                            </ds:X509IssuerSerial>
                        </ds:X509Data>
                    </wsse:SecurityTokenReference>");
            
            // Act
            var reference = new IssuerSecurityTokenReference(xmlDocument.DocumentElement, new StubCertificateRepository());

            // Assert
            Assert.NotNull(reference.Certificate);
            Assert.Equal("CN=AccessPointA", reference.Certificate.Issuer);
        }
    }
}