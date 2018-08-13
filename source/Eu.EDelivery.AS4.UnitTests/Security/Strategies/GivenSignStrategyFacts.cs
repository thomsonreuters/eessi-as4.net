using System.Security.Cryptography.Xml;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.Strategies
{
    public class GivenSignStrategyFacts
    {
        [Fact]
        public void ThenSignStrategySignsCorrectlyAS4Message()
        {
            // Arrange
            SignStrategy signingStrategy = CreateSignStrategyForSigning();

            // Act
            Signature signature = signingStrategy.SignDocument();

            XmlNode signatureElement = signature.GetXml();
            Assert.Equal("Signature", signatureElement.LocalName);

            XmlNodeList signedInfoReferences = signatureElement.SelectEbmsNodes("/dsig:SignedInfo/dsig:Reference");
            Assert.True(
                signedInfoReferences.Count == 2, 
                "The required 2 <Reference/> elements are not present under the <SignedInfo/> element in the <Signature/>");

            XmlNodeList keyInfoReferences = signatureElement.SelectEbmsNodes("/dsig:KeyInfo/wsse:SecurityTokenReference/wsse:Reference");
            Assert.True(
                keyInfoReferences.Count == 1,
                "The required 1 <Reference/> element is not present under the <KeyInfo/> element in the <Signature/>");
        }

        private static SignStrategy CreateSignStrategyForSigning()
        {
            var signingId = new SigningId("header-id", "body-id");
            AS4Message as4Message = AS4Message.Empty;
            as4Message.SigningId = signingId;

            var signingConfig = new CalculateSignatureConfig(new StubCertificateRepository().GetStubCertificate(),
                X509ReferenceType.BSTReference, Constants.SignAlgorithms.Sha256, Constants.HashFunctions.Sha256);

            return SignStrategy.ForAS4Message(as4Message, signingConfig);
        }
    }
}
