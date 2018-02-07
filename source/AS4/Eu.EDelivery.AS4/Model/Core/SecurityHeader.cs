using System;
using System.Collections;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Xml;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Strategies;

namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// WS Security Signed Xml
    /// </summary>
    public class SecurityHeader
    {
        public bool IsSigned { get; private set; }
        public bool IsEncrypted { get; private set; }

        private XmlElement _securityHeaderElement;

        private XmlElement _signatureElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityHeader"/> class. 
        /// Create empty <see cref="SecurityHeader"/>
        /// </summary>
        public SecurityHeader() { }

        public SecurityHeader(XmlElement securityHeaderElement)
        {
            _securityHeaderElement = securityHeaderElement;

            if (_securityHeaderElement != null)
            {
                var nsMgr = new XmlNamespaceManager(_securityHeaderElement.OwnerDocument.NameTable);

                nsMgr.AddNamespace("ds", Constants.Namespaces.XmlDsig);
                nsMgr.AddNamespace("xenc", Constants.Namespaces.XmlEnc);

                _signatureElement = _securityHeaderElement.SelectSingleNode("//ds:Signature", nsMgr) as XmlElement;
                IsSigned = _signatureElement != null;

                IsEncrypted = _securityHeaderElement.SelectSingleNode("//xenc:EncryptedData", nsMgr) != null;
            }
            else
            {
                IsSigned = false;
                IsEncrypted = false;
            }
        }

        private Signature _signature;

        /// <summary>
        /// Sign using the given <paramref name="signingStrategy"/>
        /// </summary>
        /// <param name="signingStrategy"></param>
        public void Sign( ICalculateSignatureStrategy signingStrategy)
        {
            if (signingStrategy == null)
            {
                throw new ArgumentNullException(nameof(signingStrategy));
            }

            _signature = signingStrategy.SignDocument();

            IsSigned = true;

            //var securityHeader = CreateSecurityHeaderElement();

            //if (_securityHeaderElement == null)
            //{
            //    _securityHeaderElement = CreateSecurityHeaderElement();
            //}

            // The SecurityToken that was used for the signature must occur before the 
            // signature and its references.
            //foreach (SecurityTokenReference reference in signature.KeyInfo.OfType<SecurityTokenReference>())
            //{
            //    reference.AppendSecurityTokenTo(securityHeader, _securityHeaderElement.OwnerDocument);
            //}

            //var signatureElement = signature.GetXml();
            //_signatureElement = _securityHeaderElement.OwnerDocument.ImportNode(signatureElement, deep: true) as XmlElement;
            //_securityHeaderElement.AppendChild(_signatureElement);
        }

        private XmlElement _encryptionElement;

        /// <summary>
        /// Encrypts the message and its attachments.
        /// </summary>
        /// <param name="encryptionStrategy"></param>
        public void Encrypt(IEncryptionStrategy encryptionStrategy)
        {
            if (encryptionStrategy == null)
            {
                throw new ArgumentNullException(nameof(encryptionStrategy));
            }

            encryptionStrategy.EncryptMessage();
            IsEncrypted = true;

            //if (_securityHeaderElement == null)
            //{
            //    _securityHeaderElement = CreateSecurityHeaderElement();
            //}
            _encryptionElement = CreateSecurityHeaderElement();

            encryptionStrategy.AppendEncryptionElements(_encryptionElement);
        }

        private static XmlElement CreateSecurityHeaderElement()
        {
            var xmlDocument = new XmlDocument() { PreserveWhitespace = true };
            return xmlDocument.CreateElement("wsse", "Security", Constants.Namespaces.WssSecuritySecExt);
        }

        /// <summary>
        /// Gets the full Security XML element.
        /// </summary>
        /// <returns></returns>
        public XmlElement GetXml()
        {
            var securityHeader = CreateSecurityHeaderElement();

            if (_encryptionElement != null)
            {
                securityHeader = _encryptionElement;
            }

            if (_signature != null)
            {
                // The SecurityToken that was used for the signature must occur before the 
                // signature and its references.
                foreach (SecurityTokenReference reference in _signature.KeyInfo.OfType<SecurityTokenReference>())
                {
                    reference.AppendSecurityTokenTo(securityHeader, securityHeader.OwnerDocument);
                }

                var signatureElement = _signature.GetXml();
                signatureElement = 
                    securityHeader.OwnerDocument.ImportNode(signatureElement, deep: true) as XmlElement;
                securityHeader.AppendChild(signatureElement);
            }

            return securityHeader;
        }

        /// <summary>
        /// Get the Signed References from the signature.
        /// </summary>
        /// <returns></returns>
        public ArrayList GetReferences()
        {
            // TODO: this must be improved.
            if (_signatureElement == null)
            {
                return new ArrayList();
            }

            SignedXml x = new SignedXml(_signatureElement);
            x.LoadXml(_signatureElement);
            return x.SignedInfo.References;
        }
    }
}