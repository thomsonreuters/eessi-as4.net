using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.References
{
    /// <summary>
    /// Testing <see cref="IssuerSecurityTokenReference" />
    /// </summary>
    public class GivenIssuerSecurityTokenReferenceFacts
    {
        private readonly X509Certificate2 _dummyCertificate;
        private readonly IssuerSecurityTokenReference _reference;

        public GivenIssuerSecurityTokenReferenceFacts()
        {
            var certRepository = new StubCertificateRepository();
            _dummyCertificate = certRepository.GetStubCertificate();
            _reference = new IssuerSecurityTokenReference(certRepository.GetStubCertificate());
        }

        [Fact]
        public void ThenGetXmlContainsSecurityTokenReference()
        {
            // Act
            XmlElement xml = _reference.GetXml();

            // Assert
            Assert.NotNull(xml);
            Assert.Equal("wsse:SecurityTokenReference", xml.Name);
            Assert.Equal(Constants.Namespaces.WssSecuritySecExt, xml.NamespaceURI);

            XmlNode issuerName = xml.SelectEbmsNode("/dsig:X509Data/dsig:X509IssuerSerial/dsig:X509IssuerName");
            Assert.Equal(_dummyCertificate.IssuerName.Name, issuerName.InnerText);

            XmlNode serialNumberNode = xml.SelectEbmsNode("/dsig:X509Data/dsig:X509IssuerSerial/dsig:X509SerialNumber");
            string expectedSerialNumber = Convert.ToUInt64($"0x{_dummyCertificate.SerialNumber}", 16).ToString();
            Assert.Equal(expectedSerialNumber, serialNumberNode.InnerText);
        }

        [Fact]
        public void ThenLoadXmlGetsTheCertificateFromTheXml()
        {
            // Arrange and Act
            var reference = new IssuerSecurityTokenReference(GetDummyXml(), new StubCertificateRepository());

            // Assert
            Assert.Equal(_dummyCertificate, reference.Certificate);
        }

        private XmlElement GetDummyXml()
        {
            var xmlDocument = new XmlDocument {PreserveWhitespace = true};

            XmlElement securityTokenReferenceElement = xmlDocument.CreateElement(
                "SecurityTokenReference",
                Constants.Namespaces.WssSecuritySecExt);
            XmlElement x509DataElement = Createx509DataElement(xmlDocument, securityTokenReferenceElement);
            XmlElement x509IssuerSerialElement = Createx509IssuerSerialElement(xmlDocument, x509DataElement);
            Createx509IssuerNameElement(xmlDocument, x509IssuerSerialElement);
            Createx509SerialNumberElement(xmlDocument, x509IssuerSerialElement);

            return securityTokenReferenceElement;
        }

        private static XmlElement Createx509DataElement(XmlDocument xmlDocument, XmlElement securityTokenReferenceElement)
        {
            XmlElement x509DataElement = xmlDocument.CreateElement("X509Data");
            securityTokenReferenceElement.AppendChild(x509DataElement);

            return x509DataElement;
        }

        private static XmlElement Createx509IssuerSerialElement(XmlDocument xmlDocument, XmlElement x509DataElement)
        {
            XmlElement x509IssuerSerialElement = xmlDocument.CreateElement("X509IssuerSerial");
            x509DataElement.AppendChild(x509IssuerSerialElement);

            return x509IssuerSerialElement;
        }

        private void Createx509IssuerNameElement(XmlDocument xmlDocument, XmlElement x509IssuerSerialElement)
        {
            XmlElement x509IssuerNameElement = xmlDocument.CreateElement("X509IssuerName");
            x509IssuerSerialElement.AppendChild(x509IssuerNameElement);
            x509IssuerNameElement.InnerText = _dummyCertificate.IssuerName.ToString();
        }

        private void Createx509SerialNumberElement(XmlDocument xmlDocument, XmlElement x509IssuerSerialElement)
        {
            XmlElement x509SerialNumberElement = xmlDocument.CreateElement("X509SerialNumber");
            x509IssuerSerialElement.AppendChild(x509SerialNumberElement);
            x509SerialNumberElement.InnerText = _dummyCertificate.SerialNumber ?? string.Empty;
        }
    }
}