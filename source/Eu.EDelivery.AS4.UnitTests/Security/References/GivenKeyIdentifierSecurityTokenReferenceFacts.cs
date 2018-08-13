using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.References
{
    /// <summary>
    /// Testing the <see cref="KeyIdentifierSecurityTokenReference" />
    /// </summary>s
    public class GivenKeyIdentifierSecurityTokenReferenceFacts
    {
        private readonly KeyIdentifierSecurityTokenReference _reference;

        public GivenKeyIdentifierSecurityTokenReferenceFacts()
        {
            var repository = new StubCertificateRepository();

            _reference = new KeyIdentifierSecurityTokenReference(repository.GetStubCertificate());
        }

        [Fact]
        public void ThenGetXmlContainsKeyIdentifier()
        {
            // Arrange
            const string subjectKeyIdentifier = "hRmOyHw/oLIBBsGKp/L9qzCUZ1k=";

            // Act
            XmlElement xml = _reference.GetXml();

            // Assert
            Assert.NotNull(xml);
            Assert.Equal("wsse:SecurityTokenReference", xml.Name);
            Assert.Equal(Constants.Namespaces.WssSecuritySecExt, xml.NamespaceURI);

            XmlNode keyIdentifierNode = xml.SelectEbmsNode("/wsse:KeyIdentifier");
            Assert.Equal(subjectKeyIdentifier, keyIdentifierNode.InnerText);
            Assert.NotNull(keyIdentifierNode.Attributes);
            Assert.Collection(
                keyIdentifierNode.Attributes.Cast<XmlAttribute>(),
                a1 => a1.AssertEbmsAttribute("EncodingType", Constants.Namespaces.Base64Binary),
                a2 => a2.AssertEbmsAttribute("ValueType", Constants.Namespaces.SubjectKeyIdentifier));
        }

        /// <summary>
        /// Testing if the Reference Succeeds for the "LoadXml" Method
        /// </summary>
        public class GivenValidArgumentsForLoadXml : GivenKeyIdentifierSecurityTokenReferenceFacts
        {
            [Fact]
            [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "Test for null")]
            public void ThenLoadXmlGetsCertificateFromXml()
            {
                // Arrange and Act
                var reference = new KeyIdentifierSecurityTokenReference(_reference.GetXml(), new StubCertificateRepository());

                Assert.NotNull(reference.Certificate);
                Assert.Equal(_reference.Certificate, reference.Certificate);
            }
        }
    }
}