using System.Xml;
using Xunit;
using Xunit.Sdk;

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
        /// Asserts the select XML node.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="localName">Name of the local.</param>
        /// <returns></returns>
        public static XmlNode AssertXmlNodeNotNull(this XmlDocument document, string localName)
        {
            XmlNode node = document.SelectSingleNode($"//*[local-name()='{localName}']");
            Assert.NotNull(node);

            return node;
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

    public class XmlDocumentExtensionsFacts
    {
        [Fact]
        public void AssertXmlNode_Succeeds()
        {
            // Arrange
            var sut = new XmlDocument();
            sut.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><Person></Person>");
            XmlNode expected = sut.FirstChild;

            // Act
            XmlNode actual = sut.AssertXmlNodeNotNull("Person");

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AssertXmlNode_Fails()
        {
            // Arrange
            var sut = new XmlDocument();

            // Act / Assert
            Assert.Throws<NotNullException>(() => sut.AssertXmlNodeNotNull("Person"));
        }
    }
}