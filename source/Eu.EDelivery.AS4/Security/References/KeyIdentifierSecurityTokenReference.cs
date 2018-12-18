using System;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.Security.References
{
    /// <summary>
    /// Security Token Reference Strategy for the Key Identifier
    /// </summary>
    internal sealed class KeyIdentifierSecurityTokenReference : SecurityTokenReference
    {
        private readonly ICertificateRepository _certificateRepository;
        private string _certificateSubjectKeyIdentifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyIdentifierSecurityTokenReference"/> class. 
        /// </summary>
        /// <param name="certificate">The Certificate for which a SecurityTokenReference must be created.</param>
        public KeyIdentifierSecurityTokenReference(X509Certificate2 certificate)
        {
            Certificate = certificate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyIdentifierSecurityTokenReference"/> class.
        /// </summary>
        /// <param name="envelope">XML Element that contains a Key Identifier Security Token Reference.</param>
        /// <param name="certificateRepository">Repository to obtain the certificate needed to embed it into the Key Identifier Security Token Reference.</param>
        public KeyIdentifierSecurityTokenReference(XmlElement envelope, ICertificateRepository certificateRepository)
        {
            _certificateRepository = certificateRepository;
            LoadXml(envelope);
        }

        protected override X509Certificate2 LoadCertificate()
        {
            if (String.IsNullOrWhiteSpace(_certificateSubjectKeyIdentifier))
            {
                throw new InvalidOperationException("Unable to retrieve Certificate: No SubjectKeyIdentifier available.");
            }

            if (_certificateRepository == null)
            {
                throw new InvalidOperationException("Unable to retrieve Certificate: No CertificateRepository defined.");
            }

            return _certificateRepository.GetCertificate(
                X509FindType.FindBySubjectKeyIdentifier, _certificateSubjectKeyIdentifier);
        }

        /// <summary>
        /// Load the <see cref="X509Certificate2" />
        /// from the given <paramref name="element" />
        /// </summary>
        /// <param name="element"></param>
        public override void LoadXml(XmlElement element)
        {
            var ns = new XmlNamespaceManager(new NameTable());
            ns.AddNamespace("wsse", Constants.Namespaces.WssSecuritySecExt);

            var xmlKeyIdentifier = element.SelectSingleNode("//wsse:SecurityTokenReference/wsse:KeyIdentifier", ns) as XmlElement;
            if (xmlKeyIdentifier == null)
            {
                throw new XmlException(
                    "No <wsse:KeyIdentifier/> element found in <wsse:SecurityTokenReference/> element");
            }

            byte[] base64Bytes = Convert.FromBase64String(xmlKeyIdentifier.InnerText);
            var soapHexBinary = new SoapHexBinary(base64Bytes);

            _certificateSubjectKeyIdentifier = soapHexBinary.ToString();
        }

        /// <summary>
        /// Get the Xml for the Key Identifier
        /// </summary>
        /// <returns></returns>
        public override XmlElement GetXml()
        {
            var xmlDocument = new XmlDocument { PreserveWhitespace = true };

            XmlElement securityTokenReferenceElement = xmlDocument.CreateElement(
                prefix: "wsse",
                localName: "SecurityTokenReference",
                namespaceURI: Constants.Namespaces.WssSecuritySecExt);

            XmlElement keyIdentifierElement = xmlDocument.CreateElement(
                prefix: "wsse",
                localName: "KeyIdentifier",
                namespaceURI: Constants.Namespaces.WssSecuritySecExt);

            SetKeyIdentifierSecurityAttributes(keyIdentifierElement);
            keyIdentifierElement.InnerText = GetSubjectKeyIdentifier();
            securityTokenReferenceElement.AppendChild(keyIdentifierElement);

            return securityTokenReferenceElement;
        }

        private static void SetKeyIdentifierSecurityAttributes(XmlElement keyIdentifierElement)
        {
            keyIdentifierElement.SetAttribute("EncodingType", Constants.Namespaces.Base64Binary);
            keyIdentifierElement.SetAttribute("ValueType", Constants.Namespaces.SubjectKeyIdentifier);
        }

        private string GetSubjectKeyIdentifier()
        {
            if (!String.IsNullOrWhiteSpace(_certificateSubjectKeyIdentifier))
            {
                return _certificateSubjectKeyIdentifier;
            }

            foreach (X509Extension extension in Certificate.Extensions)
            {
                if (IsExtensionNotSubjectKeyIdentifier(extension))
                {
                    continue;
                }

                return RetrieveBinary64SubjectKeyIdentifier(extension);
            }

            return string.Empty;
        }

        private static string RetrieveBinary64SubjectKeyIdentifier(X509Extension extension)
        {
            var x509SubjectKeyIdentifierExtension = (X509SubjectKeyIdentifierExtension)extension;
            SoapHexBinary base64Binary = SoapHexBinary.Parse(x509SubjectKeyIdentifierExtension.SubjectKeyIdentifier);

            return Convert.ToBase64String(base64Binary.Value);
        }

        private static bool IsExtensionNotSubjectKeyIdentifier(AsnEncodedData extension)
        {
            return !string.Equals(extension.Oid.FriendlyName, "Subject Key Identifier");
        }

    }
}