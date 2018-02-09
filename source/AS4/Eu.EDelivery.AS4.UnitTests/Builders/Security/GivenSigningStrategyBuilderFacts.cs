using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Security;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;
using CryptoReference = System.Security.Cryptography.Xml.Reference;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Security
{
    /// <summary>
    /// Testing the <see cref="SigningStrategyBuilder" />
    /// </summary>
    [Obsolete("SigningStrategyBuilder is obsolete which makes these tests obsolete as well.")]
    public class GivenSigningStrategyBuilderFacts
    {
        private SigningStrategyBuilder _builder;

        /// <summary>
        /// Testing if the Builder Succeeds
        /// </summary>
        public class GivenValidArgumentsBuilder : GivenSigningStrategyBuilderFacts
        {
            [Fact(Skip = "SigningStrategyBuilder is obsolete")]
            public void ThenBuilderLoadsInfoFromXmlDocument()
            {
                // Arrange
                string algorithmNamespace = Constants.SignAlgorithms.Sha256;
                var xmlDocument = new XmlDocument();
                string xml = Properties.Resources.as4_soap_signed_message;
                xmlDocument.LoadXml(xml);

                // Act
                _builder = new SigningStrategyBuilder(xmlDocument);
                ISigningStrategy signingStrategy = _builder.Build();

                // Assert
                var concreteStrategy = signingStrategy as SigningStrategy;
                Assert.IsType<BinarySecurityTokenReference>(concreteStrategy?.SecurityTokenReference);

                var signedXml = signingStrategy as SignedXml;
                Assert.NotNull(signedXml);
                Assert.Equal(algorithmNamespace, signedXml.SignedInfo.SignatureMethod);
            }

            [Fact(Skip = "SigningStrategyBuilder is obsolete")]
            public void ThenBuilderMakesValidEmptySignStrategy()
            {
                // Act
                ISigningStrategy signingStrategy = CreateBuilder().Build();

                // Assert
                Assert.NotNull(signingStrategy);
            }

            [Fact(Skip = "SigningStrategyBuilder is obsolete")]
            public void ThenBuilderMakesValidSignStrategyWithAttachmentReference()
            {
                // Arrange
                var stream = new MemoryStream(Encoding.UTF8.GetBytes("Dummy Content"));
                var attachment = new Attachment("earth") { Content = stream };
                string hashFunction = Constants.HashFunctions.Sha256;

                // Act
                ISigningStrategy signingStrategy = CreateBuilder().WithAttachment(attachment, hashFunction).Build();

                // Assert
                IEnumerable<CryptoReference> references = signingStrategy.GetSignedReferences().Cast<CryptoReference>();
                AssertReference("cid:" + attachment.Id, references);
            }

            [Fact(Skip = "SigningStrategyBuilder is obsolete")]
            public void ThenBuilderMakesValidSignStrategyWithCertificate()
            {
                // Arrange
                X509Certificate2 certificate = new StubCertificateRepository().GetStubCertificate();

                // Act
                ISigningStrategy signingStrategy = CreateBuilder().WithCertificate(certificate, X509ReferenceType.BSTReference).Build();

                // Assert
                var concreteStrategy = signingStrategy as SigningStrategy;
                Assert.NotNull(concreteStrategy?.SecurityTokenReference.Certificate);
            }

            [Fact(Skip = "SigningStrategyBuilder is obsolete")]
            public void ThenBuilderMakesValidSignStrategyWithSecurityTokenReference()
            {
                // Act
                ISigningStrategy signingStrategy =
                    CreateBuilder().Build();

                // Assert
                Assert.NotNull(signingStrategy);
                var concreteStrategy = signingStrategy as SigningStrategy;
                Assert.NotNull(concreteStrategy?.SecurityTokenReference);
            }

            [Fact(Skip = "SigningStrategyBuilder is obsolete")]
            public void ThenBuilderMakesValidSignStrategyWithSignatureAlgorithm()
            {
                // Arrange
                string algorithmNamespace = Constants.SignAlgorithms.Sha256;

                // Act
                ISigningStrategy signingStrategy = CreateBuilder().WithSignatureAlgorithm(algorithmNamespace).Build();

                // Assert
                Assert.NotNull(signingStrategy);
                var signedXml = signingStrategy as SignedXml;
                Assert.NotNull(signedXml);
                Assert.Equal(algorithmNamespace, signedXml.SignedInfo.SignatureMethod);
            }

            [Fact(Skip = "SigningStrategyBuilder is obsolete")]
            public void ThenBuilerMakesValidSignStrategyWithSigningId()
            {
                // Arrange
                var signingId = new SigningId("header-id", "body-id");
                string hashFunction = Constants.HashFunctions.Sha256;

                // Act
                ISigningStrategy signingStrategy = CreateBuilder().WithSigningId(signingId, hashFunction).Build();

                // Assert
                IEnumerable<CryptoReference> references = signingStrategy.GetSignedReferences().Cast<CryptoReference>();
                AssertReference("#" + signingId.HeaderSecurityId, references);
                AssertReference("#" + signingId.BodySecurityId, references);
            }

            private static void AssertReference(string uri, IEnumerable<CryptoReference> references)
            {
                string hashFunction = Constants.HashFunctions.Sha256;
                CryptoReference reference =
                    references.FirstOrDefault(r => r.Uri.Equals(uri) && r.DigestMethod.Equals(hashFunction));

                Assert.NotNull(reference);
            }
        }

        public class GivenInvalidArguments : GivenSigningStrategyBuilderFacts
        {
            [Fact(Skip = "SigningStrategyBuilder is obsolete")]
            public void ThenBuilderFailsWithMissingSecurityTokenReferenceXmlElement()
            {
                // Arrange
                string xml = "<?xml version=\"1.0\" encoding=\"utf - 8]\"?>"
                             + $"<s12:Envelope xmlns:s12=\"{Constants.Namespaces.Soap12}\"></s12:Envelope>";

                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xml);

                // Act / Assert
                Assert.ThrowsAny<Exception>(() => _builder = new SigningStrategyBuilder(xmlDocument));
            }
        }

        protected SigningStrategyBuilder CreateBuilder()
        {
            return new SigningStrategyBuilder(AS4Message.Empty)
                .WithCertificate(new StubCertificateRepository().GetStubCertificate(), X509ReferenceType.BSTReference);
        }
    }
}