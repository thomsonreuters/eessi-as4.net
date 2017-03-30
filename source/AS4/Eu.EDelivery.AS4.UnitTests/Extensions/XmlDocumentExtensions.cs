using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Eu.EDelivery.AS4.UnitTests.Extensions
{
    public static class XmlDocumentExtensions
    {
        private static readonly XmlNamespaceManager NamespaceManager = new XmlNamespaceManager(new NameTable());

        static XmlDocumentExtensions()
        {
            NamespaceManager.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");
            NamespaceManager.AddNamespace("ebms", "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/");
        }

        public static XmlNode SelectXmlNode(this XmlDocument xmlDocument, string xpath)
        {
            return xmlDocument.SelectSingleNode(xpath, NamespaceManager);
        }

        public static IEnumerable<XmlNode> SelectXmlNodes(this XmlDocument xmlDocument, string xpath)
        {
            return xmlDocument.SelectNodes(xpath, NamespaceManager)?.Cast<XmlElement>();
        }
    }
}