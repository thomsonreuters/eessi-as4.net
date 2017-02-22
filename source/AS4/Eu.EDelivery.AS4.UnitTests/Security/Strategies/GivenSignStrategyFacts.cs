using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Security;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Mappings.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Algorithms;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.UnitTests.Common;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.Strategies
{
    /// <summary>
    /// Testing <see cref="SigningStrategy"/>
    /// </summary>
    public class GivenSignStrategyFacts
    {
        private readonly Mock<ICertificateRepository> _mockedCertificateRepository;

        public GivenSignStrategyFacts()
        {
            this._mockedCertificateRepository = new Mock<ICertificateRepository>();
        }

        public class GivenValidArguments : GivenSignStrategyFacts
        {
            [Fact]
            public void ThenSignStrategyVerifiesAS4MessageCorrectly()
            {
                // Arrange
                ISigningStrategy signingStrategy = CreateSignStrategyForVerifing();
                VerifyConfig options = base.CreateVerifyConfig();

                // Act
                bool isValid = signingStrategy.VerifySignature(options);
                // Assert
                Assert.True(isValid);
            }

            private ISigningStrategy CreateSignStrategyForVerifing()
            {
                XmlDocument xmlDocument = CreateXmlDocument();
                SecurityTokenReference reference = CreateSecurityTokenReference(xmlDocument);

                var signStrategy = new SigningStrategy(xmlDocument, reference);
                signStrategy.AddAlgorithm(new RsaPkCs1Sha256SignatureAlgorithm());

                return signStrategy;
            }

            private static SecurityTokenReference CreateSecurityTokenReference(XmlDocument xmlDocument)
            {
                SecurityTokenReference reference = new BinarySecurityTokenReference();
                XmlNode securityTokenElement =
                    xmlDocument.SelectSingleNode("//*[local-name()='SecurityTokenReference'] ");
                reference.LoadXml((XmlElement) securityTokenElement);

                return reference;
            }

            private XmlDocument CreateXmlDocument()
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(Properties.Resources.as4_soap_signed_message);

                return xmlDocument;
            }

            [Fact]
            public void ThenSignStrategySignsCorrectlyAS4Message()
            {
                // Arrange
                ISigningStrategy signingStrategy = CreateSignStrategyForSigning();
                // Act
                signingStrategy.SignSignature();
                // Assert
                XmlElement securityElement = CreateSecurityElement();
                signingStrategy.AppendSignature(securityElement);

                AssertSecurityElement(securityElement);
            }

            private ISigningStrategy CreateSignStrategyForSigning()
            {
                var signingId = new SigningId("header-id", "body-id");
                var as4Message = new AS4Message {SigningId = signingId};

                XmlDocument xmlDocument = SerializeAS4Message(as4Message);
                ISigningStrategy signingStrategy = CreateDefaultSignStrategy(signingId, xmlDocument);
                return signingStrategy;
            }

            private void AssertSecurityElement(XmlNode securityElement)
            {
                XmlNode xmlSignature = securityElement.SelectSingleNode("//*[local-name()='Signature'] ");
                Assert.NotNull(xmlSignature);

                XmlNodeList xmlReferences = xmlSignature.SelectNodes("//*[local-name()='Reference'] ");
                Assert.NotNull(xmlReferences);
                Assert.True(xmlReferences.Count == 3);
            }

            private SigningStrategy CreateDefaultSignStrategy(SigningId signingId, XmlDocument xmlDocument)
            {
                var signStrategy = new SigningStrategy(xmlDocument, new BinarySecurityTokenReference());

                signStrategy.AddAlgorithm(new RsaPkCs1Sha256SignatureAlgorithm());                
                signStrategy.AddCertificate(new StubCertificateRepository().GetDummyCertificate());
                signStrategy.AddXmlReference(signingId.HeaderSecurityId, Constants.HashFunctions.First());
                signStrategy.AddXmlReference(signingId.BodySecurityId, Constants.HashFunctions.First());

                return signStrategy;
            }
        }

        public class GivenInvalidArgumens : GivenSignStrategyFacts
        {
            ////[Fact]
            ////public void ThenVerifySignatureFailsWithInvalidXmlDocument()
            ////{
            ////    // Arrange
            ////    var xmlDocument = new XmlDocument();
            ////    xmlDocument.LoadXml(Properties.Resources.as4_soap_signed_message);
            ////    var signStrategy = new SigningStrategy(base.CreateSecurityElement().OwnerDocument);

            ////    ConfigureDefaultSignStrategy(xmlDocument, signStrategy);

            ////    VerifyConfig options = base.CreateVerifyConfig();

            ////    // Act / Assert
            ////    AS4Exception as4Exception = Assert.Throws<AS4Exception>(()
            ////        => signStrategy.VerifySignature(options));  
            ////    Assert.Equal(ErrorCode.Ebms0101, as4Exception.ErrorCode);
            ////}

            [Fact]
            public void ThenVerifySignatureFailsWithUntrustedCertificate()
            {
                // Arrange
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(Properties.Resources.as4_soap_untrusted_signed_message);

                var signStrategy = ConfigureDefaultSignStrategy(xmlDocument);

                //var signStrategy = new SigningStrategy(xmlDocument);
                //ConfigureDefaultSignStrategy(xmlDocument, signStrategy);

                VerifyConfig options = base.CreateVerifyConfig();

                // Act / Assert
                var as4Exception = Assert.Throws<AS4Exception>(()
                    => signStrategy.VerifySignature(options));
                Assert.Equal(ErrorCode.Ebms0101, as4Exception.ErrorCode);
            }

            private SigningStrategy ConfigureDefaultSignStrategy(XmlDocument document)
            {
                var builder = new SigningStrategyBuilder(document);
                builder.WithSignatureAlgorithm(new RsaPkCs1Sha256SignatureAlgorithm());

                return (SigningStrategy)builder.Build();
            }

            private void ConfigureDefaultSignStrategy(XmlDocument xmlDocument, SigningStrategy signingStrategy)
            {
                //SecurityTokenReference reference = new BinarySecurityTokenReference();
                //XmlNode securityTokenElement =
                //    xmlDocument.SelectSingleNode("//*[local-name()='SecurityTokenReference'] ");

                //reference.LoadXml((XmlElement) securityTokenElement);
                //signingStrategy.SecurityTokenReference = reference;
                //signingStrategy.AddAlgorithm(new RsaPkCs1Sha256SignatureAlgorithm());
            }
        }

        protected XmlDocument SerializeAS4Message(AS4Message as4Message)
        {
            var memoryStream = new MemoryStream();
            var provider = new SerializerProvider();
            ISerializer serializer = provider.Get(Constants.ContentTypes.Soap);
            serializer.Serialize(as4Message, memoryStream, CancellationToken.None);

            return LoadEnvelopeToDocument(memoryStream);
        }

        private XmlDocument LoadEnvelopeToDocument(Stream envelopeStream)
        {
            envelopeStream.Position = 0;
            var envelopeXmlDocument = new XmlDocument();
            var readerSettings = new XmlReaderSettings {CloseInput = false};

            using (XmlReader reader = XmlReader.Create(envelopeStream, readerSettings))
                envelopeXmlDocument.Load(reader);

            return envelopeXmlDocument;
        }

        protected XmlElement CreateSecurityElement()
        {
            var xmlSecurityHeader = new XmlDocument();
            XmlElement securityElement = xmlSecurityHeader.CreateElement("Security",
                Constants.Namespaces.WssSecuritySecExt);

            return securityElement;
        }

        protected VerifyConfig CreateVerifyConfig()
        {
            var options = new VerifyConfig();            
            options.Attachments = new List<Attachment>();

            return options;
        }
    }
}