using System.Xml;

namespace Eu.EDelivery.AS4.Security.References
{
    public class ReferenceSecurityTokenReference : SecurityTokenReference
    {
        public ReferenceSecurityTokenReference(string referenceId)
        {
            this.ReferenceId = referenceId;
        }

        /// <summary>
        /// The ValueType attribute of the reference
        /// </summary>
        public override XmlElement GetXml()
        {
            var xmlDocument = new XmlDocument { PreserveWhitespace = true };

            XmlElement securityTokenReferenceElement = xmlDocument.CreateElement(
                "SecurityTokenReference",
                Constants.Namespaces.WssSecuritySecExt);

            XmlElement referenceElement = xmlDocument.CreateElement("Reference", Constants.Namespaces.WssSecuritySecExt);
            securityTokenReferenceElement.AppendChild(referenceElement);

            referenceElement.SetAttribute("URI", "#" + this.ReferenceId);

            return securityTokenReferenceElement;
        }

        public override void LoadXml(XmlElement element)
        {
            throw new System.NotImplementedException();
        }
    }
}
