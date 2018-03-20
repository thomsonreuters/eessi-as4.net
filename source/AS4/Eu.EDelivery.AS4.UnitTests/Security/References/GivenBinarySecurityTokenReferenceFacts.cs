using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.UnitTests.Common;
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
                Assert.Equal("BinarySecurityToken", xmlElement.FirstChild.Name);
                Assert.Equal(Constants.Namespaces.WssSecuritySecExt, xmlElement.FirstChild.NamespaceURI);
            }

            [Fact]
            public void ThenSecurityTokenContainsEncodingTypeAttribute()
            {
                // Arrange
                XmlDocument xmlDocument = CreateSecurityHeaderDocument();
                XmlElement securityHeaderElement = GetSecurityHeaderElement(xmlDocument);                

                // Act
                XmlElement xmlElement = _reference.AppendSecurityTokenTo(securityHeaderElement, xmlDocument);

                // Assert
                Assert.NotNull(xmlElement);
                Assert.NotNull(xmlElement.FirstChild.Attributes);
                Assert.Equal("EncodingType", xmlElement.FirstChild.Attributes[0].Name);
                Assert.Equal(Constants.Namespaces.Base64Binary, xmlElement.FirstChild.Attributes[0].Value);
            }

            [Fact]
            public void ThenSecurityTokenContainsIdAttribute()
            {
                // Arrange
                XmlDocument xmlDocument = CreateSecurityHeaderDocument();

                // Act
                XmlElement xmlElement = _reference.AppendSecurityTokenTo(
                    xmlDocument.FirstChild as XmlElement,
                    xmlDocument);

                // Assert
                Assert.NotNull(xmlElement);
                Assert.NotNull(xmlElement.FirstChild.Attributes);
                Assert.Equal("Id", xmlElement.FirstChild.Attributes[2].Name);
                Assert.Equal(Constants.Namespaces.WssSecurityUtility, xmlElement.FirstChild.Attributes[2].NamespaceURI);
                Assert.Equal(_reference.ReferenceId, xmlElement.FirstChild.Attributes[2].Value);
            }

            [Fact]
            public void ThenSecurityTokenContainsValueTypeAttribute()
            {
                // Arrange
                XmlDocument xmlDocument = CreateSecurityHeaderDocument();
                XmlElement securityHeaderElement = GetSecurityHeaderElement(xmlDocument);                

                // Act
                XmlElement xmlElement = _reference.AppendSecurityTokenTo(securityHeaderElement, xmlDocument);

                // Assert
                Assert.NotNull(xmlElement);
                Assert.NotNull(xmlElement.FirstChild.Attributes);
                Assert.Equal("ValueType", xmlElement.FirstChild.Attributes[1].Name);
                Assert.Equal(Constants.Namespaces.ValueType, xmlElement.FirstChild.Attributes[1].Value);
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
                Assert.Equal("Reference", xmlElement.FirstChild.LocalName);
                Assert.Equal(Constants.Namespaces.WssSecuritySecExt, xmlElement.FirstChild.NamespaceURI);
            }

            [Fact]
            public void ThenXmlContainsSecurityTokenReferenceElement()
            {
                // Act
                XmlElement xmlElement = _reference.GetXml();

                // Assert
                Assert.NotNull(xmlElement);
                Assert.Equal("SecurityTokenReference", xmlElement.LocalName);
                Assert.Equal(Constants.Namespaces.WssSecuritySecExt, xmlElement.NamespaceURI);
            }

            [Fact]
            public void ThenXmlReferenceContainsURIAttribute()
            {
                // Act
                XmlElement xmlElement = _reference.GetXml();

                // Assert
                Assert.NotNull(xmlElement);
                Assert.NotNull(xmlElement.FirstChild.Attributes);

                var uriAttribute = xmlElement.FirstChild.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.LocalName == "URI");

                Assert.NotNull(uriAttribute);
                Assert.Equal($"#{_reference.ReferenceId}", uriAttribute.Value);
            }

            [Fact]
            public void ThenXmlReferenceContainsValueTypeAttribute()
            {
                // Act
                XmlElement xmlElement = _reference.GetXml();

                // Assert
                Assert.NotNull(xmlElement);
                Assert.NotNull(xmlElement.FirstChild.Attributes);

                var valueTypeAttribute = 
                    xmlElement.FirstChild.Attributes.OfType<XmlAttribute>()
                                                    .FirstOrDefault(a => a.Name.Equals("ValueType"));

                Assert.NotNull(valueTypeAttribute);
                Assert.Equal(Constants.Namespaces.ValueType, valueTypeAttribute.Value);
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

                // Act
                _reference.LoadXml((XmlElement)xmlDocument.SelectSingleNode("//*[local-name()='SecurityTokenReference']"));

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

                return ibmSecurityHeader.SelectNodes("//*[local-name()='SecurityTokenReference']").Item(1) as XmlElement;
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
            xmlDocument.AppendChild(xmlDocument.CreateElement("Security"));
            return xmlDocument;
        }

        protected XmlElement GetSecurityHeaderElement(XmlDocument xmlDocument)
        {
            return xmlDocument.FirstChild as XmlElement;
        }
    }
}