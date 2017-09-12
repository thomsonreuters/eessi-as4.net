using System;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Services.PullRequestAuthorization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.TestUtils;
using Eu.EDelivery.AS4.UnitTests.Services;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    public class GivenVerifyPullRequestAuthorizationStepFacts
    {
        [Fact]
        public async Task ContinuesExecution_IfMatchedCertificateCanBeFoundForTheMpc()
        {
            // Arrange
            var signingCertificate = GetSigningCertificate();

            const string expectedMpc = "message-mpc";
            MessagingContext context = ContextWithSignedPullRequest(expectedMpc, signingCertificate);

            var stubMap = new StubAuthorizationMapProvider(new[] { new PullRequestAuthorizationEntry(expectedMpc, signingCertificate.Thumbprint, true) });
            var sut = new VerifyPullRequestAuthorizationStep(stubMap);

            // Act
            StepResult result = await sut.ExecuteAsync(context, CancellationToken.None);

            // Assert
            Assert.True(result.CanProceed);
        }

        [Fact]
        public async Task FailsToAuthorize_WhenNoCertificateMatchesMpc()
        {
            // Arrange
            var signingCertificate = GetSigningCertificate();

            const string expectedMpc = "message-mpc";
            MessagingContext context = ContextWithSignedPullRequest(expectedMpc, signingCertificate);

            var stubMap = new StubAuthorizationMapProvider(new[] { new PullRequestAuthorizationEntry(expectedMpc, "ANOTHERTHUMBPRINT", true) });
            var sut = new VerifyPullRequestAuthorizationStep(stubMap);

            // Act and assert.
            await Assert.ThrowsAsync<SecurityException>(() => sut.ExecuteAsync(context, CancellationToken.None));
        }

        private static X509Certificate2 GetSigningCertificate()
        {
            var cert = new X509Certificate2(Properties.Resources.holodeck_partya_certificate,
                                           Properties.Resources.certificate_password, X509KeyStorageFlags.Exportable);

            Assert.NotNull(cert.PrivateKey);

            return cert;
        }

        private static MessagingContext ContextWithSignedPullRequest(string expectedMpc, X509Certificate2 signingCertificate)
        {
            AS4Message message = AS4Message.Create(new PullRequest(expectedMpc));

            message = AS4MessageUtils.SignWithCertificate(message, signingCertificate);

            return new MessagingContext(message, MessagingContextMode.Send);
        }
    }
}
