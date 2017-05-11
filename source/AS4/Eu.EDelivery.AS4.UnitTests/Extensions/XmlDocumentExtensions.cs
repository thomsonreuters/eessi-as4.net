using System.Xml;

namespace Eu.EDelivery.AS4.UnitTests.Extensions
{
    /// <summary>
    /// Extensions for the <see cref="XmlDocument"/>.
    /// </summary>
    public static class XmlDocumentExtensions
    {
        private static readonly XmlNamespaceManager NamespaceManager = new XmlNamespaceManager(new NameTable());

        /// <summary>
        /// Initializes static members of the <see cref="XmlDocumentExtensions"/> class.
        /// </summary>
        static XmlDocumentExtensions()
        {
            NamespaceManager.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");
            NamespaceManager.AddNamespace("ebms", "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/");
        }

        /// <summary>
        /// Selects the XML node.
        /// </summary>
        /// <param name="xmlDocument">The XML document.</param>
        /// <param name="xpath">The xpath.</param>
        /// <returns></returns>
        public static XmlNode SelectXmlNode(this XmlDocument xmlDocument, string xpath)
        {
            return xmlDocument.SelectSingleNode(xpath, NamespaceManager);
        }
    }
}