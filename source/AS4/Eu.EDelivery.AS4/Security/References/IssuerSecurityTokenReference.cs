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
    internal class IssuerSecurityTokenReference : SecurityTokenReference
    {
        private readonly string _securityTokenReferenceId;
        private readonly string _keyInfoId;
        private readonly ICertificateRepository _certifcateRepository;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssuerSecurityTokenReference"/> class. 
        /// Create a new <see cref="SecurityTokenReference"/>
        /// to handle <see cref="X509ReferenceType.IssuerSerial"/> configuration
        /// </summary>
        public IssuerSecurityTokenReference(ICertificateRepository certificateRepository)
        {
            this._keyInfoId = $"KI-{Guid.NewGuid()}";
            this._securityTokenReferenceId = $"STR-{Guid.NewGuid()}";
            this._logger = LogManager.GetCurrentClassLogger();
            this._certifcateRepository = certificateRepository;
        }

        public IssuerSecurityTokenReference(XmlElement envelope, ICertificateRepository certifcateRepository)
        {
            // First assign _certificateRepository since LoadXml will use this member.            
            _certifcateRepository = certifcateRepository;

            LoadXml(envelope);
        }

        /// <summary>
        /// Add a KeyInfo Id to the <KeyInfo/> Element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public override XmlElement AppendSecurityTokenTo(XmlElement element, XmlDocument document)
        {
            var nodeKeyInfo = (XmlElement)element.SelectSingleNode("//*[local-name()='KeyInfo']");
            nodeKeyInfo?.SetAttribute("Id", Constants.Namespaces.WssSecurityUtility, this._keyInfoId);

            return element;
        }

        /// <summary>
        /// When overridden in a derived class, returns an XML representation of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoClause" />.
        /// </summary>
        /// <returns>An XML representation of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoClause" />.</returns>
        public override XmlElement GetXml()
        {
            var xmlDocument = new XmlDocument { PreserveWhitespace = true };

            XmlElement securityTokenReferenceElement = CreateSecurityTokenReferenceElement(xmlDocument);

            XmlElement x509DataElement = CreateX509DataElement(xmlDocument, securityTokenReferenceElement);
            XmlElement x509IssuerSerialElement = CreateX509IssuerSerialElement(xmlDocument, x509DataElement);
            CreateX509IssuerNameElement(xmlDocument, x509IssuerSerialElement);
            CreateX509SerialNumberElement(xmlDocument, x509IssuerSerialElement);

            return securityTokenReferenceElement;
        }

        private XmlElement CreateSecurityTokenReferenceElement(XmlDocument xmlDocument)
        {
            XmlElement securityTokenReferenceElement = xmlDocument
                .CreateElement("wsse", "SecurityTokenReference", Constants.Namespaces.WssSecuritySecExt);

            securityTokenReferenceElement.SetAttribute(
                "Id", Constants.Namespaces.WssSecurityUtility, this._securityTokenReferenceId);

            return securityTokenReferenceElement;
        }

        private XmlElement CreateX509DataElement(XmlDocument xmlDocument, XmlElement securityTokenReferenceElement)
        {
            XmlElement x509DataElement = xmlDocument
                .CreateElement("ds", "X509Data", Constants.Namespaces.XmlDsig);
            securityTokenReferenceElement.AppendChild(x509DataElement);

            return x509DataElement;
        }

        private XmlElement CreateX509IssuerSerialElement(XmlDocument xmlDocument, XmlElement x509DataElement)
        {
            XmlElement x509IssuerSerialElement = xmlDocument
                .CreateElement("ds", "X509IssuerSerial", Constants.Namespaces.XmlDsig);
            x509DataElement.AppendChild(x509IssuerSerialElement);

            return x509IssuerSerialElement;
        }

        private void CreateX509IssuerNameElement(XmlDocument xmlDocument, XmlElement x509IssuerSerialElement)
        {
            XmlElement x509IssuerNameElement = xmlDocument
                .CreateElement("ds", "X509IssuerName", Constants.Namespaces.XmlDsig);
            x509IssuerSerialElement.AppendChild(x509IssuerNameElement);
            x509IssuerNameElement.InnerText = base.Certificate.IssuerName.Name;
        }

        private void CreateX509SerialNumberElement(XmlDocument xmlDocument, XmlElement x509IssuerSerialElement)
        {
            XmlElement x509SerialNumberElement = xmlDocument
                .CreateElement("ds", "X509SerialNumber", Constants.Namespaces.XmlDsig);
            x509IssuerSerialElement.AppendChild(x509SerialNumberElement);
            x509SerialNumberElement.InnerText = TryGetIssuerSerialNumber();
        }

        private string TryGetIssuerSerialNumber()
        {
            try
            {
                return Convert.ToUInt64($"0x{base.Certificate.SerialNumber}", 16).ToString();
            }
            catch (Exception)
            {
                this._logger.Debug($"Failed to convert {base.Certificate.SerialNumber} to ulong.");
                return string.Empty;
            }
        }

        /// <summary>
        /// Load the <see cref="X509Certificate2"/> 
        /// from the given <paramref name="element"/>
        /// </summary>
        /// <param name="element"></param>
        public override void LoadXml(XmlElement element)
        {
            var xmlIssuerSerial = (XmlElement)element.SelectSingleNode(".//*[local-name()='X509SerialNumber']");

            this.Certificate = this._certifcateRepository.GetCertificate(X509FindType.FindBySerialNumber, xmlIssuerSerial.InnerText);
        }
    }
}