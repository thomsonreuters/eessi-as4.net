using System;
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
        private readonly BinarySecurityTokenReference _reference;
        private readonly string _dummyReferenceId;
        private readonly X509Certificate2 _dummyCertificate;

        public GivenBinarySecurityTokenReferenceFacts()
        {
            this._dummyReferenceId = $"#{Guid.NewGuid()}";
            this._dummyCertificate = new StubCertificateRepository().GetDummyCertificate();
            this._reference = new BinarySecurityTokenReference();
        }

        protected XmlElement GetDummySecurityToken(XmlDocument document)
        {
            XmlElement securityTokenReferenceElement = document
                .CreateElement("SecurityTokenReference", Constants.Namespaces.WssSecuritySecExt);

            XmlElement referenceElement = AddReferenceElement(document);
            securityTokenReferenceElement.AppendChild(referenceElement);

            return securityTokenReferenceElement;
        }

        private XmlElement AddReferenceElement(XmlDocument document)
        {
            XmlElement referenceElement = document
                .CreateElement("Reference", Constants.Namespaces.WssSecuritySecExt);

            referenceElement.SetAttribute("ValueType", Constants.Namespaces.ValueType);
            referenceElement.SetAttribute("URI", this._dummyReferenceId);
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

        /// <summary>
        /// Testing the Reference with valid arguments
        /// </summary>
        public class GivenValidArgumentsForGetSecurityToken : GivenBinarySecurityTokenReferenceFacts
        {
            [Fact]
            public void ThenBinarySecurityTokenIsAddedToXmlDocumentArgument()
            {
                // Arrange
                XmlDocument xmlDocument = base.CreateSecurityHeaderDocument();
                XmlElement securityHeaderElement = base.GetSecurityHeaderElement(xmlDocument);
                base._reference.Certificate = base._dummyCertificate;
                // Act
                XmlElement xmlElement = base._reference.AddSecurityTokenTo(securityHeaderElement, xmlDocument);
                // Assert
                Assert.Equal(xmlDocument, xmlElement.OwnerDocument);
            }

            [Fact]
            public void ThenSecurityTokenContainsBinarySecurityToken()
            {
                // Arrange
                XmlDocument xmlDocument = base.CreateSecurityHeaderDocument();
                XmlElement securityHeaderElement = base.GetSecurityHeaderElement(xmlDocument);
                base._reference.Certificate = base._dummyCertificate;
                // Act
                XmlElement xmlElement = base._reference.AddSecurityTokenTo(securityHeaderElement, xmlDocument);
                // Assert
                Assert.NotNull(xmlElement);
                Assert.Equal("BinarySecurityToken", xmlElement.FirstChild.Name);
                Assert.Equal(Constants.Namespaces.WssSecuritySecExt, xmlElement.FirstChild.NamespaceURI);
            }

            [Fact]
            public void ThenSecurityTokenContainsEncodingTypeAttribute()
            {
                // Arrange
                XmlDocument xmlDocument = base.CreateSecurityHeaderDocument();
                XmlElement securityHeaderElement = base.GetSecurityHeaderElement(xmlDocument);
                base._reference.Certificate = base._dummyCertificate;
                // Act
                XmlElement xmlElement = base._reference.AddSecurityTokenTo(securityHeaderElement, xmlDocument);
                // Assert
                Assert.NotNull(xmlElement);
                Assert.NotNull(xmlElement.FirstChild.Attributes);
                Assert.Equal("EncodingType", xmlElement.FirstChild.Attributes[0].Name);
                Assert.Equal(Constants.Namespaces.Base64Binary, xmlElement.FirstChild.Attributes[0].Value);
            }

            [Fact]
            public void ThenSecurityTokenContainsValueTypeAttribute()
            {
                // Arrange
                XmlDocument xmlDocument = base.CreateSecurityHeaderDocument();
                XmlElement securityHeaderElement = base.GetSecurityHeaderElement(xmlDocument);
                base._reference.Certificate = base._dummyCertificate;
                // Act
                XmlElement xmlElement = base._reference.AddSecurityTokenTo(securityHeaderElement, xmlDocument);
                // Assert
                Assert.NotNull(xmlElement);
                Assert.NotNull(xmlElement.FirstChild.Attributes);
                Assert.Equal("ValueType", xmlElement.FirstChild.Attributes[1].Name);
                Assert.Equal(Constants.Namespaces.ValueType, xmlElement.FirstChild.Attributes[1].Value);
            }

            [Fact]
            public void ThenSecurityTokenContainsIdAttribute()
            {
                // Arrange
                XmlDocument xmlDocument = base.CreateSecurityHeaderDocument();
                base._reference.Certificate = base._dummyCertificate;
                // Act
                XmlElement xmlElement = base._reference.AddSecurityTokenTo(xmlDocument.FirstChild as XmlElement, xmlDocument);
                // Assert
                Assert.NotNull(xmlElement);
                Assert.NotNull(xmlElement.FirstChild.Attributes);
                Assert.Equal("Id", xmlElement.FirstChild.Attributes[2].Name);
                Assert.Equal(Constants.Namespaces.WssSecurityUtility, xmlElement.FirstChild.Attributes[2].NamespaceURI);
                Assert.Equal(base._reference.ReferenceId, xmlElement.FirstChild.Attributes[2].Value);
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
                XmlElement xmlElement = base._reference.GetXml();
                // Assert
                Assert.NotNull(xmlElement);
                Assert.Equal("Reference", xmlElement.FirstChild.LocalName);
                Assert.Equal(Constants.Namespaces.WssSecuritySecExt, xmlElement.FirstChild.NamespaceURI);
            }

            [Fact]
            public void ThenXmlContainsSecurityTokenReferenceElement()
            {
                // Act
                XmlElement xmlElement = base._reference.GetXml();
                // Assert
                Assert.NotNull(xmlElement);
                Assert.Equal("SecurityTokenReference", xmlElement.LocalName);
                Assert.Equal(Constants.Namespaces.WssSecuritySecExt, xmlElement.NamespaceURI);
            }

            [Fact]
            public void ThenXmlReferenceContainsURIAttribute()
            {
                // Act
                XmlElement xmlElement = base._reference.GetXml();
                // Assert
                Assert.NotNull(xmlElement);
                Assert.NotNull(xmlElement.FirstChild.Attributes);
                Assert.Equal("URI", xmlElement.FirstChild.Attributes[1].Name);
                Assert.Equal($"#{base._reference.ReferenceId}", xmlElement.FirstChild.Attributes[1].Value);
            }

            [Fact]
            public void ThenXmlReferenceContainsValueTypeAttribute()
            {
                // Act
                XmlElement xmlElement = base._reference.GetXml();
                // Assert
                Assert.NotNull(xmlElement);
                Assert.NotNull(xmlElement.FirstChild.Attributes);
                Assert.Equal("ValueType", xmlElement.FirstChild.Attributes[0].Name);
                Assert.Equal(Constants.Namespaces.ValueType, xmlElement.FirstChild.Attributes[0].Value);
            }
        }

        /// <summary>
        /// Testing the Reference with valid Arguments for the "LoadXml" Method
        /// </summary>
        public class GivenValidArgumentsForLoadXml : GivenBinarySecurityTokenReferenceFacts
        {
            [Fact]
            public void ThenLoadXmlFindsReferenceId()
            {
                // Arrange
                var xmlDocument = new XmlDocument();
                XmlElement securityTokenElement = base.GetDummySecurityToken(xmlDocument);
                // Act
                base._reference.LoadXml(securityTokenElement);
                // Assert
                Assert.Equal(base._dummyReferenceId, base._reference.ReferenceId);
            }

            [Fact]
            public void ThenLoadXmlCorrectlyLoadCertificate()
            {
                // Arrange
                XmlDocument xmlDocument = base.CreateSecurityHeaderDocument();
                XmlElement securityTokenElement = base.GetDummySecurityToken(xmlDocument);
                BuildSecurityHeader(xmlDocument, securityTokenElement);

                // Act
                base._reference.LoadXml(securityTokenElement);
                // Assert
                Assert.Equal(base._dummyCertificate, base._reference.Certificate);
            }

            private void BuildSecurityHeader(XmlDocument xmlDocument, XmlElement securityTokenElement)
            {
                XmlElement securityHeader = CreateSecurityHeader(xmlDocument);
                XmlElement securityHeaderElement = AppendSecurityHeader(xmlDocument, securityHeader);
                XmlElement signatureElement = AppendSignature(xmlDocument, securityHeaderElement);
                XmlElement keyInfoElement = AppendKeyInfoElement(xmlDocument, signatureElement);

                keyInfoElement.AppendChild(securityTokenElement);
            }

            private XmlElement CreateSecurityHeader(XmlDocument xmlDocument)
            {
                base._reference.Certificate = base._dummyCertificate;
                XmlElement securityHeaderElement = base.GetSecurityHeaderElement(xmlDocument);
                XmlElement securityHeader = base._reference.AddSecurityTokenTo(securityHeaderElement, xmlDocument);
                base._reference.Certificate = null;
                return securityHeader;
            }

            private XmlElement AppendSecurityHeader(XmlDocument xmlDocument, XmlNode securityHeader)
            {
                XmlElement securityHeaderElement = xmlDocument.CreateElement("SecurityHeader");
                securityHeaderElement.AppendChild(securityHeader.FirstChild);
                return securityHeaderElement;
            }

            private XmlElement AppendSignature(XmlDocument xmlDocument, XmlNode securityHeaderElement)
            {
                XmlElement signatureElement = xmlDocument.CreateElement("Signature");
                securityHeaderElement.AppendChild(signatureElement);
                return signatureElement;
            }

            private XmlElement AppendKeyInfoElement(XmlDocument xmlDocument, XmlNode signatureElement)
            {
                XmlElement keyInfoElement = xmlDocument.CreateElement("KeyInfo");
                signatureElement.AppendChild(keyInfoElement);
                return keyInfoElement;
            }
        }

        /// <summary>
        /// Testing the Reference with invalid Arguments for "GetSecurityToken" Method
        /// </summary>
        public class GivenInvalidArgumentsForGetSecurityToken : GivenBinarySecurityTokenReferenceFacts
        {
            [Fact]
            public void ThenGetSecurityTokenFailsWithUnsettedCertificate()
            {
                // Arrange
                var xmlDocument = new XmlDocument();
                xmlDocument.AppendChild(xmlDocument.CreateElement("Security"));
                XmlElement securityHeaderElement = base.GetSecurityHeaderElement(xmlDocument);

                // Act / Assert
                Assert.Throws<NullReferenceException>(() 
                    => base._reference.AddSecurityTokenTo(securityHeaderElement, xmlDocument));
            }
        }
    }
}