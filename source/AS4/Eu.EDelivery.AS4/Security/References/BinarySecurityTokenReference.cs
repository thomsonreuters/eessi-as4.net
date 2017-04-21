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
        /// Initializes a new instance of the <see cref="BinarySecurityTokenReference"/> class.
        /// </summary>
        public BinarySecurityTokenReference() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="BinarySecurityTokenReference"/> class.
        /// </summary>
        /// <param name="securityTokenElement">The 'Security' element to append the <see cref="BinarySecurityTokenReference"/>.</param>
        public BinarySecurityTokenReference(XmlElement securityTokenElement)
        {
            LoadXml(securityTokenElement);
        }

        /// <summary>
        /// Load the Xml from Element into Binary Security Token Reference
        /// </summary>
        /// <param name="element"></param>
        public override sealed void LoadXml(XmlElement element)
        {
            XmlElement referenceElement = GetReferenceElement(element);

            if (referenceElement != null)
            {
                AssignReferenceId(referenceElement);
            }

            XmlElement binarySecurityTokenElement = GetBinarySecurityTokenElementFrom(element);
            if (binarySecurityTokenElement != null)
            {
                AssignCertificate(binarySecurityTokenElement);
            }
        }

        private static XmlElement GetReferenceElement(XmlNode node)
        {
            return
                node.ChildNodes.OfType<XmlElement>()
                    .FirstOrDefault(
                        e => e.LocalName == "Reference" && e.NamespaceURI == Constants.Namespaces.WssSecuritySecExt);
        }

        private void AssignReferenceId(XmlElement referenceElement)
        {
            if (referenceElement.LocalName.Equals("Reference", StringComparison.OrdinalIgnoreCase) == false)
            {
                throw new ArgumentException(
                    @"The given XmlElement is not a Reference element",
                    nameof(referenceElement));
            }

            XmlAttribute uriAttribute = referenceElement.Attributes["URI"];
            if (uriAttribute != null)
            {
                ReferenceId = uriAttribute.Value;
            }
        }

        private XmlElement GetBinarySecurityTokenElementFrom(XmlNode node)
        {
            XmlNode securityHeader = node.ParentNode?.ParentNode?.ParentNode;
            return securityHeader?.ChildNodes.OfType<XmlElement>().FirstOrDefault(IsElementABinarySecurityTokenElement);
        }

        private bool IsElementABinarySecurityTokenElement(XmlElement x)
        {
            // Extra check on ReferenceId. 
            XmlAttribute idAttribute = x.Attributes["Id", Constants.Namespaces.WssSecurityUtility];
            string pureId = ReferenceId.Replace("#", string.Empty);

            return x.LocalName == "BinarySecurityToken" && idAttribute?.Value == pureId
                   && x.NamespaceURI == Constants.Namespaces.WssSecuritySecExt;
        }

        private void AssignCertificate(XmlNode binarySecurityTokenElement)
        {
            byte[] base64String = Convert.FromBase64String(binarySecurityTokenElement.InnerText);
            Certificate = new X509Certificate2(base64String);
        }

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
            binarySecurityToken.SetAttribute("Id", Constants.Namespaces.WssSecurityUtility, ReferenceId);
        }

        private void AppendCertificate(XmlDocument document, XmlElement binarySecurityToken)
        {
            if (Certificate != null)
            {
                string rawData = Convert.ToBase64String(Certificate.GetRawCertData());
                XmlNode rawDataNode = document.CreateTextNode(rawData);
                binarySecurityToken.AppendChild(rawDataNode);
            }
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
            referenceElement.SetAttribute("URI", "#" + ReferenceId);
        }
    }
}