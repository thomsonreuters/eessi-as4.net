using System.Xml;
using Xunit;

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
            NamespaceManager.AddNamespace("s12", Constants.Namespaces.Soap12);
            NamespaceManager.AddNamespace("eb", Constants.Namespaces.EbmsXmlCore);
            NamespaceManager.AddNamespace("ebbp", Constants.Namespaces.EbmsXmlSignals);
            NamespaceManager.AddNamespace("mh", Constants.Namespaces.EbmsMultiHop);
            NamespaceManager.AddNamespace("wsa", Constants.Namespaces.Addressing);
            NamespaceManager.AddNamespace("wsse", Constants.Namespaces.WssSecuritySecExt);
            NamespaceManager.AddNamespace("wsu", Constants.Namespaces.WssSecurityUtility);
            NamespaceManager.AddNamespace("dsig", Constants.Namespaces.XmlDsig);
        }

        /// <summary>
        /// Selects the XML node.
        /// </summary>
        /// <param name="xmlDocument">The XML document.</param>
        /// <param name="xpath">The xpath.</param>
        /// <returns></returns>
        public static XmlNode UnsafeSelectEbmsNode(this XmlDocument xmlDocument, string xpath)
        {
            return xmlDocument.SelectSingleNode(xpath, NamespaceManager);
        }

        /// <summary>
        /// Selects the XML node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public static XmlNode UnsafeSelectEbmsNode(this XmlNode node, string xpath)
        {
            return node.SelectSingleNode(xpath, NamespaceManager);
        }

        /// <summary>
        /// Asserts on the presence of a XPath query selection on the specified <paramref name="node"/>.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public static XmlNodeList SelectEbmsNodes(this XmlNode node, string xpath)
        {
            XmlNodeList result = node.SelectNodes(xpath, NamespaceManager);
            Assert.True(
                result != null,
                $"XPath query: \n\n {xpath} \n\n doesn't have a result on: \n\n {node.OuterXml}");

            return result;
        }

        /// <summary>
        /// Asserts on the presence of a XPath query selection on the specified <paramref name="node"/>.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public static XmlNode SelectEbmsNode(this XmlNode node, string xpath)
        {
            XmlNode result = UnsafeSelectEbmsNode(node, xpath);
            Assert.True(
                result != null, 
                $"XPath query: \n\n {xpath} \n\n doesn't have a result on: \n\n {node.OuterXml}");

            return result;
        }

        /// <summary>
        /// Asserts of the presence of a specified <paramref name="name"/> and <paramref name="value"/> in the XML attribute.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void AssertEbmsAttribute(this XmlAttribute a, string name, string value)
        {
            Assert.Equal(name, a.Name);
            Assert.Equal(value, a.Value);
        }
    }
}