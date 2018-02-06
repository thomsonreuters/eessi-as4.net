using System;
using System.Globalization;
using System.Numerics;
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
        private readonly ICertificateRepository _certificateRepository;
        private readonly string _keyInfoId;
        private readonly string _securityTokenReferenceId;

        private string _certificateSerialNr;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssuerSecurityTokenReference" /> class.
        /// to handle <see cref="X509ReferenceType.IssuerSerial" /> configuration
        /// </summary>
        /// <param name="certificate">The certificate for which a SecurityTokenReference needs to be created.</param>
        public IssuerSecurityTokenReference(X509Certificate2 certificate)
        {
            _keyInfoId = $"KI-{Guid.NewGuid()}";
            _securityTokenReferenceId = $"STR-{Guid.NewGuid()}";
            Certificate = certificate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IssuerSecurityTokenReference"/>
        /// </summary>
        /// <param name="envelope">The Xml element that contains the securitytoken SecurityToken.</param>
        /// <param name="certificateRepository">Repository to obtain the certificate that is needed.</param>
        public IssuerSecurityTokenReference(XmlElement envelope, ICertificateRepository certificateRepository)
        {
            _certificateRepository = certificateRepository;
            LoadXml(envelope);
        }

        protected override X509Certificate2 LoadCertificate()
        {
            if (String.IsNullOrWhiteSpace(_certificateSerialNr))
            {
                throw new InvalidOperationException("Unable to retrieve Certificate: No X509SerialNumber available.");
            }

            if (_certificateRepository == null)
            {
                throw new InvalidOperationException("Unable to retrieve Certificate: No CertificateRepository defined.");
            }

            return _certificateRepository.GetCertificate(X509FindType.FindBySerialNumber, _certificateSerialNr);
        }

        /// <summary>
        /// Load the <see cref="X509Certificate2" />
        /// from the given <paramref name="element" />
        /// </summary>
        /// <param name="element"></param>
        public override void LoadXml(XmlElement element)
        {
            var xmlIssuerSerial = (XmlElement)element.SelectSingleNode(".//*[local-name()='X509SerialNumber']");

            _certificateSerialNr = xmlIssuerSerial?.InnerText;
        }

        /// <summary>
        /// Add a KeyInfo Id to the <KeyInfo /> Element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public override XmlElement AppendSecurityTokenTo(XmlElement element, XmlDocument document)
        {
            var nodeKeyInfo = (XmlElement)element.SelectSingleNode("//*[local-name()='KeyInfo']");
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
            if (Certificate != null)
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
        }

        private void CreateX509SerialNumberElement(XmlDocument xmlDocument, XmlNode x509IssuerSerialElement)
        {
            XmlElement x509SerialNumberElement = xmlDocument.CreateElement(
                prefix: "ds",
                localName: "X509SerialNumber",
                namespaceURI: Constants.Namespaces.XmlDsig);

            if (!String.IsNullOrWhiteSpace(_certificateSerialNr))
            {
                x509SerialNumberElement.InnerText = _certificateSerialNr;
            }
            else
            {
                x509SerialNumberElement.InnerText = TryGetIssuerSerialNumber(Certificate);
            }

            x509IssuerSerialElement.AppendChild(x509SerialNumberElement);
        }

        private static string TryGetIssuerSerialNumber(X509Certificate2 certificate)
        {
            try
            {
                BigInteger.TryParse(
                    value: certificate.SerialNumber,
                    style: NumberStyles.AllowHexSpecifier,
                    provider: CultureInfo.InvariantCulture,
                    result: out var serialNumber);

                return serialNumber.ToString();
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