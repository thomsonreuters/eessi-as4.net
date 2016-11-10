using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace Eu.EDelivery.AS4.Security.References
{
    /// <summary>
    /// Binary Security Token Strategy to add a Security Reference to the Message
    /// </summary>
    internal class BinarySecurityTokenReference : SecurityTokenReference
    {
        /// <summary>
        /// Append the Security Token Reference for the Binary Security Token
        /// </summary>
        /// <param name="element"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public override XmlElement AppendSecurityTokenTo(XmlElement element, XmlDocument document)
        {
            XmlElement securityTokenElement = GetSecurityToken(document);
            element.AppendChild(securityTokenElement);
            return element;
        }

        private XmlElement GetSecurityToken(XmlDocument document)
        {
            XmlElement binarySecurityToken = document.CreateElement(
                "BinarySecurityToken",
                Constants.Namespaces.WssSecuritySecExt);

            SetBinarySecurityAttributes(binarySecurityToken);
            AppendCertificate(document, binarySecurityToken);

            return binarySecurityToken;
        }

        private void SetBinarySecurityAttributes(XmlElement binarySecurityToken)
        {
            binarySecurityToken.SetAttribute("EncodingType", Constants.Namespaces.Base64Binary);
            binarySecurityToken.SetAttribute("ValueType", Constants.Namespaces.ValueType);
            binarySecurityToken.SetAttribute("Id", Constants.Namespaces.WssSecurityUtility, this.ReferenceId);
        }

        private void AppendCertificate(XmlDocument document, XmlElement binarySecurityToken)
        {
            string rawData = Convert.ToBase64String(this.Certificate.GetRawCertData());
            XmlNode rawDataNode = document.CreateTextNode(rawData);
            binarySecurityToken.AppendChild(rawDataNode);
        }

        /// <summary>
        /// Get Binary Security Token Reference Xml Element
        /// </summary>
        /// <returns></returns>
        public override XmlElement GetXml()
        {
            var xmlDocument = new XmlDocument { PreserveWhitespace = true };

            XmlElement securityTokenReferenceElement = xmlDocument.CreateElement(
                "SecurityTokenReference",
                Constants.Namespaces.WssSecuritySecExt);

            XmlElement referenceElement = xmlDocument.CreateElement("Reference", Constants.Namespaces.WssSecuritySecExt);
            securityTokenReferenceElement.AppendChild(referenceElement);
            SetReferenceSecurityAttributes(referenceElement);

            return securityTokenReferenceElement;
        }

        private void SetReferenceSecurityAttributes(XmlElement referenceElement)
        {
            referenceElement.SetAttribute("ValueType", Constants.Namespaces.ValueType);
            referenceElement.SetAttribute("URI", "#" + this.ReferenceId);
        }

        /// <summary>
        /// Load the Xml from Element into Binary Security Token Reference
        /// </summary>
        /// <param name="element"></param>
        public override void LoadXml(XmlElement element)
        {
            if (IsElementAReferenceElement(element))
                AssignReferenceId(element);

            XmlElement binarySecurityTokenElement = GetBinarySecurityTokenElementFrom(element);
            if (binarySecurityTokenElement != null)
                AssignCertificate(binarySecurityTokenElement);
        }

        private void AssignCertificate(XmlElement binarySecurityTokenElement)
        {
            byte[] base64String = Convert.FromBase64String(binarySecurityTokenElement.InnerText);
            this.Certificate = new X509Certificate2(base64String);
        }

        private void AssignReferenceId(XmlElement element)
        {
            XmlAttribute uriAttribute = element.FirstChild.Attributes?["URI"];
            if (uriAttribute != null)
                this.ReferenceId = uriAttribute.Value;
        }

        private XmlElement GetBinarySecurityTokenElementFrom(XmlNode node)
        {
            XmlNode securityHeader = node.ParentNode?.ParentNode?.ParentNode;
            return securityHeader?.ChildNodes?.OfType<XmlElement>()
                .FirstOrDefault(IsElementABinarySecurityTokenElement);
        }

        private bool IsElementABinarySecurityTokenElement(XmlElement x)
        {
            return x.LocalName == "BinarySecurityToken" &&
                   x.NamespaceURI == Constants.Namespaces.WssSecuritySecExt;
        }

        private bool IsElementAReferenceElement(XmlNode element)
        {
            return element.FirstChild.LocalName == "Reference" &&
                   element.FirstChild.NamespaceURI == Constants.Namespaces.WssSecuritySecExt;
        }
    }
}