using System;
using System.Xml;

namespace Eu.EDelivery.AS4.Security.References
{
    /// <summary>
    /// Encryption Key Info Reference Token Reference.
    /// </summary>
    public class ReferenceSecurityTokenReference : SecurityTokenReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceSecurityTokenReference"/> class.
        /// </summary>
        /// <param name="referenceId">The embedded id inside the reference.</param>
        public ReferenceSecurityTokenReference(string referenceId)
        {
            ReferenceId = referenceId;
        }

        /// <summary>
        /// The ValueType attribute of the reference
        /// </summary>
        public override XmlElement GetXml()
        {
            var xmlDocument = new XmlDocument {PreserveWhitespace = true};

            XmlElement securityTokenReferenceElement = xmlDocument.CreateElement(
                "SecurityTokenReference",
                Constants.Namespaces.WssSecuritySecExt);

            XmlElement referenceElement = xmlDocument.CreateElement("Reference", Constants.Namespaces.WssSecuritySecExt);
            securityTokenReferenceElement.AppendChild(referenceElement);

            referenceElement.SetAttribute("URI", "#" + ReferenceId);

            return securityTokenReferenceElement;
        }

        /// <summary>
        /// When overridden in a derived class, parses the input <see cref="T:System.Xml.XmlElement" /> and configures the internal state of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoClause" /> to match.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Xml.XmlElement" /> that specifies the state of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoClause" />. </param>
        public override void LoadXml(XmlElement element)
        {
            throw new NotImplementedException();
        }
    }
}