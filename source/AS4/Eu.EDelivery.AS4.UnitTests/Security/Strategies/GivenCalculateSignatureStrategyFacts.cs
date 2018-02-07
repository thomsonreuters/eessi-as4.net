using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.Strategies
{
    public class GivenCalculateSignatureStrategyFacts
    {
        [Fact]
        public void ThenSignStrategySignsCorrectlyAS4Message()
        {
            // Arrange
            ICalculateSignatureStrategy signingStrategy = CreateSignStrategyForSigning();

            // Act
            var signature = signingStrategy.SignDocument();

            AssertSecurityElement(signature.GetXml());
        }

        private static ICalculateSignatureStrategy CreateSignStrategyForSigning()
        {
            var signingId = new SigningId("header-id", "body-id");
            AS4Message as4Message = AS4Message.Empty;
            as4Message.SigningId = signingId;

            var signingConfig = new CalculateSignatureConfig(new StubCertificateRepository().GetStubCertificate(),
                X509ReferenceType.BSTReference, Constants.SignAlgorithms.Sha256, Constants.HashFunctions.Sha256);

            return CalculateSignatureStrategy.ForAS4Message(as4Message, signingConfig);
        }

        private static void AssertSecurityElement(XmlNode signatureElement)
        {
            Assert.Equal("Signature", signatureElement.LocalName);

            XmlNodeList xmlReferences = signatureElement.SelectNodes("//*[local-name()='Reference'] ");
            Assert.NotNull(xmlReferences);
            Assert.True(xmlReferences.Count == 3);
        }

    }
}
