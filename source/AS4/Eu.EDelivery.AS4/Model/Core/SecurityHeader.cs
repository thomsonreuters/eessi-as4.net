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
                var nsMgr = GetNamespaceManager(_securityHeaderElement.OwnerDocument);

                IsSigned = _securityHeaderElement.SelectSingleNode("//ds:Signature", nsMgr) != null;
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
        public void Sign(ICalculateSignatureStrategy signingStrategy)
        {
            if (signingStrategy == null)
            {
                throw new ArgumentNullException(nameof(signingStrategy));
            }

            _signature = signingStrategy.SignDocument();

            IsSigned = true;
        }

        private XmlNodeList _encryptionElements;

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

            var securityHeader = CreateSecurityHeaderElement();

            encryptionStrategy.AppendEncryptionElements(securityHeader);

            _encryptionElements = securityHeader.ChildNodes;
        }

        /// <summary>
        /// Gets the full Security XML element.
        /// </summary>
        /// <returns></returns>
        public XmlElement GetXml()
        {
            if (_securityHeaderElement == null && _signature == null && _encryptionElements == null)
            {
                return null;
            }

            if (_securityHeaderElement == null)
            {
                _securityHeaderElement = CreateSecurityHeaderElement();
            }

            // Append the encryption elements as first
            InsertNewEncryptionElements();

            // Signature elements should occur last in the header.
            InsertNewSignatureElements();

            return _securityHeaderElement;
        }

        private static XmlElement CreateSecurityHeaderElement()
        {
            var xmlDocument = new XmlDocument() { PreserveWhitespace = true };
            return xmlDocument.CreateElement("wsse", "Security", Constants.Namespaces.WssSecuritySecExt);
        }

        private void InsertNewEncryptionElements()
        {
            if (_encryptionElements == null)
            {
                return;
            }

            // Encryption elements must occur as the first items in the list.
            var referenceNode = _securityHeaderElement.ChildNodes.OfType<XmlNode>().FirstOrDefault();

            foreach (XmlNode encryptionElement in _encryptionElements)
            {
                var nodeToImport = _securityHeaderElement.OwnerDocument.ImportNode(encryptionElement, deep: true);
                _securityHeaderElement.InsertBefore(nodeToImport, referenceNode);
            }

            _encryptionElements = null;
        }

        private void InsertNewSignatureElements()
        {
            if (_signature == null)
            {
                return;
            }

            // The SecurityToken that was used for the signature must occur before the 
            // signature and its references.
            foreach (SecurityTokenReference reference in _signature.KeyInfo.OfType<SecurityTokenReference>())
            {
                reference.AppendSecurityTokenTo(_securityHeaderElement, _securityHeaderElement.OwnerDocument);
            }

            var signatureElement = _signature.GetXml();
            signatureElement =
                _securityHeaderElement.OwnerDocument.ImportNode(signatureElement, deep: true) as XmlElement;
            _securityHeaderElement.AppendChild(signatureElement);

            _signature = null;
        }

        /// <summary>
        /// Get the Signed References from the signature.
        /// </summary>
        /// <returns></returns>
        public ArrayList GetReferences()
        {
            // TODO: this must be improved.

            var securityHeader = this.GetXml();

            if (securityHeader == null)
            {
                return new ArrayList();
            }

            var signature = new SignedXml();

            var nsMgr = GetNamespaceManager(securityHeader.OwnerDocument);

            var signatureElement = securityHeader.SelectSingleNode("//ds:Signature", nsMgr) as XmlElement;

            if (signatureElement == null)
            {
                return new ArrayList();
            }

            signature.LoadXml(signatureElement);

            return signature.SignedInfo.References;
        }

        private static XmlNamespaceManager GetNamespaceManager(XmlDocument xmlDocument)
        {
            var nsMgr = new XmlNamespaceManager(xmlDocument.NameTable);

            nsMgr.AddNamespace("ds", Constants.Namespaces.XmlDsig);
            nsMgr.AddNamespace("xenc", Constants.Namespaces.XmlEnc);

            return nsMgr;
        }
    }
}