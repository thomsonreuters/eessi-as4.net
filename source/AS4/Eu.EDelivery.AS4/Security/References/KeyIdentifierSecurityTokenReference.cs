using System;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Eu.EDelivery.AS4.Exceptions;
using NLog;

namespace Eu.EDelivery.AS4.Security.References
{
    /// <summary>
    /// Security Token Reference Strategy for the Key Identifier
    /// </summary>
    internal class KeyIdentifierSecurityTokenReference : SecurityTokenReference
    {
        private readonly string _securityTokenReferenceId;
        private readonly string _keyInfoId;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyIdentifierSecurityTokenReference"/> class. 
        /// Create a new <see cref="SecurityTokenReference"/>
        /// to handle <see cref="X509ReferenceType.KeyIdentifier"/> configuration
        /// </summary>
        public KeyIdentifierSecurityTokenReference()
        {
            this._keyInfoId = $"KI-{Guid.NewGuid()}";
            this._securityTokenReferenceId = $"STR-{Guid.NewGuid()}";
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Add a KeyInfo Id to the <KeyInfo/> Element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public override XmlElement AddSecurityTokenTo(XmlElement element, XmlDocument document)
        {
            var nodeKeyInfo = (XmlElement) element.SelectSingleNode("//*[local-name()='KeyInfo']");
            nodeKeyInfo?.SetAttribute("Id", Constants.Namespaces.WssSecurityUtility, this._keyInfoId);

            return element;
        }

        /// <summary>
        /// Get the Xml for the Key Identifier
        /// </summary>
        /// <returns></returns>
        public override XmlElement GetXml()
        {
            var xmlDocument = new XmlDocument { PreserveWhitespace = true };

            XmlElement securityTokenReferenceElement = xmlDocument
                .CreateElement("wsse", "SecurityTokenReference", Constants.Namespaces.WssSecuritySecExt);

            XmlElement keyIdentifierElement = xmlDocument
                .CreateElement("wsse", "KeyIdentifier", Constants.Namespaces.WssSecuritySecExt);

            SetKeyIfentifierSecurityAttributes(keyIdentifierElement);
            keyIdentifierElement.InnerText = GetSubjectKeyIdentifier();
            securityTokenReferenceElement.AppendChild(keyIdentifierElement);

            return securityTokenReferenceElement;
        }

        private void SetKeyIfentifierSecurityAttributes(XmlElement keyIdentifierElement)
        {
            keyIdentifierElement.SetAttribute("EncodingType", Constants.Namespaces.Base64Binary);
            keyIdentifierElement.SetAttribute("ValueType", Constants.Namespaces.SubjectKeyIdentifier);
        }

        private string GetSubjectKeyIdentifier()
        {
            foreach (X509Extension extension in this.Certificate.Extensions)
            {
                if (IsExtensionNotSubjectKeyIdentifier(extension)) continue;
                return RetrieveBinary64SubjectKeyIdentifier(extension);
            }

            return string.Empty;
        }

        private string RetrieveBinary64SubjectKeyIdentifier(X509Extension extension)
        {
            var x509SubjectKeyIdentifierExtension = (X509SubjectKeyIdentifierExtension)extension;
            SoapHexBinary base64Binary = SoapHexBinary.Parse(x509SubjectKeyIdentifierExtension.SubjectKeyIdentifier);

            return Convert.ToBase64String(base64Binary.Value);
        }

        private bool IsExtensionNotSubjectKeyIdentifier(X509Extension extension)
        {
            return !string.Equals(extension.Oid.FriendlyName, "Subject Key Identifier");
        }

        /// <summary>
        /// Load the <see cref="X509Certificate2"/>
        /// from the given <paramref name="element"/>
        /// </summary>
        /// <param name="element"></param>
        public override void LoadXml(XmlElement element)
        {
            var xmlKeyIdentifier = element.SelectSingleNode("//*[local-name()='KeyIdentifier']") as XmlElement;
            if (xmlKeyIdentifier == null) throw new AS4Exception("No KeyIdentifier tag found in given XmlElement");

            SoapHexBinary soapHexBinary = RetrieveHexBinaryFromKeyIdentifier(xmlKeyIdentifier);
            SaveCertificateWithHexBinary(soapHexBinary);
        }

        private void SaveCertificateWithHexBinary(SoapHexBinary soapHexBinary)
        {
            this.Certificate = this.CertificateRepository
                .GetCertificate(X509FindType.FindBySubjectKeyIdentifier, soapHexBinary.ToString());
        }

        private SoapHexBinary RetrieveHexBinaryFromKeyIdentifier(XmlElement xmlKeyIdentifier)
        {
            byte[] base64Bytes = Convert.FromBase64String(xmlKeyIdentifier.InnerText);
            return new SoapHexBinary(base64Bytes);
        }
    }
}