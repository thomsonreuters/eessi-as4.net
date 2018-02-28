using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.Strategies
{
    public class VerifySignatureStrategyFacts
    {
        public class GivenValidArguments : VerifySignatureStrategyFacts
        {
            [Fact]
            public async Task ThenSignStrategyVerifiesAS4MessageCorrectly()
            {
                // Arrange
                var as4Message = await GetAS4Message(Properties.Resources.as4_soap_signed_message);

                // Assert to make sure that our arranged AS4Message is indeed signed.
                Assert.True(as4Message.IsSigned);

                var verificationStrategy = new SignatureVerificationStrategy(as4Message.EnvelopeDocument);

                bool validSignature = verificationStrategy.VerifySignature(AllowedUnknownRootCertAuthorityConfig());

                Assert.True(validSignature);
            }
        }

        public class GivenInvalidArguments : VerifySignatureStrategyFacts
        {
            [Fact]
            public async Task ThenVerifySignatureFailsWithUntrustedCertificate()
            {
                var as4Message = await GetAS4Message(Properties.Resources.as4_soap_untrusted_signed_message);

                // Assert to make sure that our arranged AS4Message is indeed signed.
                Assert.True(as4Message.IsSigned);

                var verificationStrategy = new SignatureVerificationStrategy(as4Message.EnvelopeDocument);

                // Act / Assert
                Assert.Throws<System.Security.Cryptography.CryptographicException>(
                    () => verificationStrategy.VerifySignature(AllowedUnknownRootCertAuthorityConfig()));
            }
        }

        private static async Task<AS4Message> GetAS4Message(string soapEnvelopeString)
        {
            var serializer = new SoapEnvelopeSerializer();

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(soapEnvelopeString)))
            {
                return await serializer.DeserializeAsync(stream, "soap/xml", CancellationToken.None);
            }
        }

        private static VerifySignatureConfig AllowedUnknownRootCertAuthorityConfig()
        {
            return new VerifySignatureConfig(allowUnknownRootCertificateAuthority: true, attachments: new List<Attachment>());
        }
    }
}
