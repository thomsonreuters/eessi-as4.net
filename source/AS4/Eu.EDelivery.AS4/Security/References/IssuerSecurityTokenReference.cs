using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Eu.EDelivery.AS4.Repositories;
using NLog;

namespace Eu.EDelivery.AS4.Security.References
{
    /// <summary>
    /// Issuer Security Strategy to add a Security Reference to the Message
    /// </summary>
    internal sealed class IssuerSecurityTokenReference : SecurityTokenReference
    {
        private readonly ICertificateRepository _certifcateRepository;
        private readonly string _keyInfoId;
        private readonly string _securityTokenReferenceId;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssuerSecurityTokenReference" /> class.
        /// to handle <see cref="X509ReferenceType.IssuerSerial" /> configuration
        /// </summary>
        /// <param name="certificateRepository">Repository to obtain the certificate needed to append to the Issuer Security Token Reference.</param>
        public IssuerSecurityTokenReference(ICertificateRepository certificateRepository)
        {
            _keyInfoId = $"KI-{Guid.NewGuid()}";
            _securityTokenReferenceId = $"STR-{Guid.NewGuid()}";
            _certifcateRepository = certificateRepository;
        }

        public IssuerSecurityTokenReference(XmlElement envelope, ICertificateRepository certifcateRepository)
        {
            // First assign _certificateRepository since LoadXml will use this member.            
            _certifcateRepository = certifcateRepository;

            LoadXml(envelope);
        }

        /// <summary>
        /// Load the <see cref="X509Certificate2" />
        /// from the given <paramref name="element" />
        /// </summary>
        /// <param name="element"></param>
        public override void LoadXml(XmlElement element)
        {
            var xmlIssuerSerial = (XmlElement)element.SelectSingleNode(".//*[local-name()='X509SerialNumber']");

            Certificate = _certifcateRepository.GetCertificate(
                X509FindType.FindBySerialNumber,
                xmlIssuerSerial?.InnerText);
        }

        /// <summary>
        /// Add a KeyInfo Id to the <KeyInfo /> Element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public override XmlElement AppendSecurityTokenTo(XmlElement element, XmlDocument document)
        {
            var nodeKeyInfo = (XmlElement) element.SelectSingleNode("//*[local-name()='KeyInfo']");
            nodeKeyInfo?.SetAttribute("Id", Constants.Namespaces.WssSecurityUtility, _keyInfoId);

            return element;
        }

        /// <summary>
        /// When overridden in a derived class, returns an XML representation of the
        /// <see cref="T:System.Security.Cryptography.Xml.KeyInfoClause" />.
        /// </summary>
        /// <returns>An XML representation of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoClause" />.</returns>
        public override XmlElement GetXml()
        {
            var xmlDocument = new XmlDocument {PreserveWhitespace = true};

            XmlElement securityTokenReferenceElement = CreateSecurityTokenReferenceElement(xmlDocument);

            XmlElement x509DataElement = CreateX509DataElement(xmlDocument, securityTokenReferenceElement);
            XmlElement x509IssuerSerialElement = CreateX509IssuerSerialElement(xmlDocument, x509DataElement);
            CreateX509IssuerNameElement(xmlDocument, x509IssuerSerialElement);
            CreateX509SerialNumberElement(xmlDocument, x509IssuerSerialElement);

            return securityTokenReferenceElement;
        }

        private XmlElement CreateSecurityTokenReferenceElement(XmlDocument xmlDocument)
        {
            XmlElement securityTokenReferenceElement = xmlDocument.CreateElement(
                prefix: "wsse",
                localName: "SecurityTokenReference",
                namespaceURI: Constants.Namespaces.WssSecuritySecExt);

            securityTokenReferenceElement.SetAttribute(
                localName: "Id",
                namespaceURI: Constants.Namespaces.WssSecurityUtility,
                value: _securityTokenReferenceId);

            return securityTokenReferenceElement;
        }

        private static XmlElement CreateX509DataElement(
            XmlDocument xmlDocument,
            XmlNode securityTokenReferenceElement)
        {
            XmlElement x509DataElement = xmlDocument.CreateElement(
                prefix: "ds", 
                localName: "X509Data", 
                namespaceURI: Constants.Namespaces.XmlDsig);

            securityTokenReferenceElement.AppendChild(x509DataElement);

            return x509DataElement;
        }

        private static XmlElement CreateX509IssuerSerialElement(XmlDocument xmlDocument, XmlElement x509DataElement)
        {
            XmlElement x509IssuerSerialElement = xmlDocument.CreateElement(
                prefix: "ds", 
                localName: "X509IssuerSerial", 
                namespaceURI: Constants.Namespaces.XmlDsig);

            x509DataElement.AppendChild(x509IssuerSerialElement);

            return x509IssuerSerialElement;
        }

        private void CreateX509IssuerNameElement(XmlDocument xmlDocument, XmlElement x509IssuerSerialElement)
        {
            XmlElement x509IssuerNameElement = xmlDocument.CreateElement(
                prefix: "ds", 
                localName: "X509IssuerName", 
                namespaceURI: Constants.Namespaces.XmlDsig);

            x509IssuerSerialElement.AppendChild(x509IssuerNameElement);

            string issuerNameName = Certificate.IssuerName.Name;
            if (issuerNameName != null)
            {
                x509IssuerNameElement.InnerText = issuerNameName;
            }
        }

        private void CreateX509SerialNumberElement(XmlDocument xmlDocument, XmlNode x509IssuerSerialElement)
        {
            XmlElement x509SerialNumberElement = xmlDocument.CreateElement(
                prefix: "ds", 
                localName: "X509SerialNumber", 
                namespaceURI: Constants.Namespaces.XmlDsig);

            x509IssuerSerialElement.AppendChild(x509SerialNumberElement);
            x509SerialNumberElement.InnerText = TryGetIssuerSerialNumber(Certificate);
        }

        private static string TryGetIssuerSerialNumber(X509Certificate2 certificate)
        {
            try
            {
                return Convert.ToUInt64($"0x{certificate.SerialNumber}", 16).ToString();
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Debug($"Failed to convert {certificate.SerialNumber} to ulong.");
                LogManager.GetCurrentClassLogger().Trace($"Exception details: {ex.Message}");

                return string.Empty;
            }
        }
    }
}