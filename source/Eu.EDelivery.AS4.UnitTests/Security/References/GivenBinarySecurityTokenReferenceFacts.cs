using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.References
{
    /// <summary>
    /// Tesing the <see cref="BinarySecurityTokenReference" />
    /// </summary>
    public class GivenBinarySecurityTokenReferenceFacts
    {
        private readonly X509Certificate2 _dummyCertificate;
        private readonly string _dummyReferenceId;
        private readonly BinarySecurityTokenReference _reference;

        public GivenBinarySecurityTokenReferenceFacts()
        {
            _dummyCertificate = new StubCertificateRepository().GetStubCertificate();

            _reference = new BinarySecurityTokenReference(_dummyCertificate);
            _dummyReferenceId = _reference.ReferenceId;
        }

        /// <summary>
        /// Testing the Reference with valid arguments
        /// </summary>
        public class GivenValidArgumentsForGetSecurityToken : GivenBinarySecurityTokenReferenceFacts
        {

            [Fact]
            public void ThenBinarySecurityTokenIsAddedToXmlDocumentArgument()
            {
                // Arrange
                XmlDocument xmlDocument = CreateSecurityHeaderDocument();
                XmlElement securityHeaderElement = GetSecurityHeaderElement(xmlDocument);                

                // Act
                XmlElement xmlElement = _reference.AppendSecurityTokenTo(securityHeaderElement, xmlDocument);

                // Assert
                Assert.Equal(xmlDocument, xmlElement.OwnerDocument);
            }

            [Fact]
            public void ThenSecurityTokenContainsBinarySecurityToken()
            {
                // Arrange
                XmlDocument xmlDocument = CreateSecurityHeaderDocument();
                XmlElement securityHeaderElement = GetSecurityHeaderElement(xmlDocument);

                // Act
                XmlElement xmlElement = _reference.AppendSecurityTokenTo(securityHeaderElement, xmlDocument);

                // Assert
                Assert.NotNull(xmlElement);
                XmlNode binarySecurityTokenTag = xmlElement.SelectEbmsNode("/wsse:Security/wsse:BinarySecurityToken");
                Assert.NotNull(binarySecurityTokenTag.Attributes);
                Assert.Collection(
                    binarySecurityTokenTag.Attributes.Cast<XmlAttribute>(),
                    a1 => a1.AssertEbmsAttribute("EncodingType", Constants.Namespaces.Base64Binary),
                    a2 => a2.AssertEbmsAttribute("ValueType", Constants.Namespaces.ValueType),
                    a3 => a3.AssertEbmsAttribute("Id", _reference.ReferenceId));
            }
        }

        /// <summary>
        /// Testing if the Binary Security Token Reference "GetXml" Method Succeeds
        /// </summary>
        public class GivenValidArgumentsForGetXml : GivenBinarySecurityTokenReferenceFacts
        {
            [Fact]
            public void ThenXmlContainsReferenceChildElement()
            {
                // Act
                XmlElement xmlElement = _reference.GetXml();

                // Assert
                Assert.NotNull(xmlElement);
                Assert.Equal("SecurityTokenReference", xmlElement.LocalName);
                Assert.Equal(Constants.Namespaces.WssSecuritySecExt, xmlElement.NamespaceURI);

                XmlNode referenceNode = xmlElement.SelectEbmsNode("/wsse:Reference");
                Assert.NotNull(referenceNode.Attributes);
                Assert.Collection(
                    referenceNode.Attributes.Cast<XmlAttribute>(),
                    a1 => a1.AssertEbmsAttribute("URI", $"#{_reference.ReferenceId}"),
                    a2 => a2.AssertEbmsAttribute("ValueType", Constants.Namespaces.ValueType));
            }
        }

        /// <summary>
        /// Testing the Reference with valid Arguments for the "LoadXml" Method
        /// </summary>
        public class GivenValidArgumentsForLoadXml : GivenBinarySecurityTokenReferenceFacts
        {
            private void BuildSecurityHeader(XmlDocument xmlDocument, XmlElement securityTokenElement)
            {
                XmlElement securityHeader = CreateSecurityHeader(xmlDocument);

                XmlElement signatureElement = xmlDocument.CreateElement("Signature");
                XmlElement keyInfoElement = xmlDocument.CreateElement("KeyInfo");
                signatureElement.AppendChild(keyInfoElement);
                securityHeader.AppendChild(signatureElement);

                keyInfoElement.AppendChild(securityTokenElement);
            }

            private XmlElement CreateSecurityHeader(XmlDocument xmlDocument)
            {                
                XmlElement securityHeaderElement = GetSecurityHeaderElement(xmlDocument);
                XmlElement securityHeader = _reference.AppendSecurityTokenTo(securityHeaderElement, xmlDocument);
             
                return securityHeader;
            }

            [Fact]
            public void ThenLoadXmlCorrectlyLoadCertificate()
            {
                // Arrange
                XmlDocument xmlDocument = CreateSecurityHeaderDocument();

                XmlElement securityTokenElement = GetDummySecurityToken(xmlDocument);

                BuildSecurityHeader(xmlDocument, securityTokenElement);

                // Act
                _reference.LoadXml(securityTokenElement);

                // Assert
                Assert.Equal(_dummyCertificate, _reference.Certificate);
            }

            [Fact]
            public void ThenLoadXmlFindsReferenceId_EvenWithExtraHashtag()
            {
                // Arrange
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(Properties.Resources.as4_soap_signed_message_with_extra_hashtag);
                var securityTokenReferenceNode = 
                    (XmlElement) xmlDocument.SelectEbmsNode("/s12:Envelope/s12:Header/wsse:Security/dsig:Signature/dsig:KeyInfo/wsse:SecurityTokenReference");

                // Act
                _reference.LoadXml(securityTokenReferenceNode);

                // Assert
                Assert.NotNull(_reference.Certificate);
            }

            [Fact]
            public void LoadsCertificate_FromIbmSecurityHeader()
            {
                // Arrange
                XmlElement securityTokenReference = GetSecurityTokenReferenceFromIbm();

                // Act
                _reference.LoadXml(securityTokenReference);

                // Assert
                Assert.NotNull(_reference.Certificate);
            }

            private static XmlElement GetSecurityTokenReferenceFromIbm()
            {
                var ibmSecurityHeader = new XmlDocument();
                ibmSecurityHeader.LoadXml(Properties.Resources.ibm_security_header);

                XmlNode securityTokenReference = 
                    ibmSecurityHeader.SelectEbmsNode("/wsse:Security/dsig:Signature/dsig:KeyInfo/wsse:SecurityTokenReference");

                return (XmlElement) securityTokenReference;
            }
        }

        protected XmlElement GetDummySecurityToken(XmlDocument document)
        {
            XmlElement securityTokenReferenceElement = document.CreateElement(
                "SecurityTokenReference",
                Constants.Namespaces.WssSecuritySecExt);

            XmlElement referenceElement = CreateReferenceElement(document);
            securityTokenReferenceElement.AppendChild(referenceElement);

            return securityTokenReferenceElement;
        }

        private XmlElement CreateReferenceElement(XmlDocument document)
        {
            XmlElement referenceElement = document.CreateElement("Reference", Constants.Namespaces.WssSecuritySecExt);

            referenceElement.SetAttribute("ValueType", Constants.Namespaces.ValueType);
            referenceElement.SetAttribute("URI", _dummyReferenceId);
            return referenceElement;
        }

        protected XmlDocument CreateSecurityHeaderDocument()
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.AppendChild(xmlDocument.CreateElement("wsse", "Security", Constants.Namespaces.WssSecuritySecExt));
            return xmlDocument;
        }

        protected XmlElement GetSecurityHeaderElement(XmlDocument xmlDocument)
        {
            return xmlDocument.FirstChild as XmlElement;
        }
    }
}