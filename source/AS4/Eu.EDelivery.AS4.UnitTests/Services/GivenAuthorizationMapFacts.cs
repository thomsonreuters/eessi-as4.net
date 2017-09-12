using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Services.PullRequestAuthorization;
using Eu.EDelivery.AS4.TestUtils;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Services
{
    public class GivenAuthorizationMapFacts
    {
        public class AuthorizedFacts : GivenAuthorizationMapFacts
        {
            [Fact]
            public void IfMpcMatchesCertificate()
            {
                var certificate = GetSigningCertificate();

                var provider = new StubAuthorizationMapProvider(new[]
                {
                    new PullRequestAuthorizationEntry("mpc1", certificate.Thumbprint, true), new PullRequestAuthorizationEntry("mpc2", certificate.Thumbprint, false)
                });

                var as4Message = CreatePullRequest("mpc1");

                var signedPullRequest = SignAS4MessageWithCertificate(as4Message, certificate);

                var service = new PullPullAuthorizationMapService(provider);

                Assert.True(service.IsPullRequestAuthorized(signedPullRequest), "PullRequest should be allowed since entry exists for MPC and cert-thumbprint");
            }

            [Fact]
            public void IfNoEntriesExistForMpcInAuthorizationMap()
            {
                var certificate = GetSigningCertificate();

                var provider = new StubAuthorizationMapProvider(new[]
                {
                    new PullRequestAuthorizationEntry("mpc1", certificate.Thumbprint, false), new PullRequestAuthorizationEntry("mpc2", certificate.Thumbprint, false)
                });

                var as4Message = CreatePullRequest("mpc3");

                var signedPullRequest = SignAS4MessageWithCertificate(as4Message, certificate);

                var service = new PullPullAuthorizationMapService(provider);

                Assert.True(service.IsPullRequestAuthorized(signedPullRequest), "PullRequest should be allowed since no entries are present for MPC3 in Authorization Map");
            }

            [Fact]
            public void IfPullRequestIsNotSignedAndNoEntriesExistForMpcInAuthorizationMap()
            {
                var certificate = GetSigningCertificate();

                var provider = new StubAuthorizationMapProvider(new[]
                {
                    new PullRequestAuthorizationEntry("mpc1", certificate.Thumbprint, false), new PullRequestAuthorizationEntry("mpc2", certificate.Thumbprint, false)
                });

                var pullRequest = CreatePullRequest("mpc3");

                var service = new PullPullAuthorizationMapService(provider);

                Assert.True(service.IsPullRequestAuthorized(pullRequest));
            }

        }

        public class NotAuthorizedFacts
        {
            [Fact]
            public void IfCertificateIsNotAllowedInAuthorizationMap()
            {
                var certificate = GetSigningCertificate();

                var provider = new StubAuthorizationMapProvider(new[]
                {
                    new PullRequestAuthorizationEntry("mpc1", certificate.Thumbprint, false), new PullRequestAuthorizationEntry("mpc2", certificate.Thumbprint, true)
                });

                var as4Message = CreatePullRequest("mpc1");

                var signedPullRequest = SignAS4MessageWithCertificate(as4Message, certificate);

                var service = new PullPullAuthorizationMapService(provider);

                Assert.False(service.IsPullRequestAuthorized(signedPullRequest), "PullRequest should not be allowed since certificate is not allowed in PullAuthorizationMap");
            }

            [Fact]
            public void IfCertificateIsNotPresentInAuthorizationMap()
            {
                var certificate = GetSigningCertificate();

                var provider = new StubAuthorizationMapProvider(new[]
                {
                    new PullRequestAuthorizationEntry("mpc1", "ABCDEFGHIJKLM", true), new PullRequestAuthorizationEntry("mpc2", certificate.Thumbprint, true)
                });

                var as4Message = CreatePullRequest("mpc1");

                var signedPullRequest = SignAS4MessageWithCertificate(as4Message, certificate);

                var service = new PullPullAuthorizationMapService(provider);

                Assert.False(service.IsPullRequestAuthorized(signedPullRequest), "PullRequest should not be allowed since certificate is not present in PullAuthorizationMap");
            }

            [Fact]
            public void IfPullRequestIsNotSignedAndEntriesExistInAuthorizationMap()
            {
                var provider = new StubAuthorizationMapProvider(new[]
                {
                    new PullRequestAuthorizationEntry("mpc1", "ABCDEFGHIJKLM", true), new PullRequestAuthorizationEntry("mpc2", "ABCDEFGHIJKLM", true)
                });

                var pullRequest = CreatePullRequest("mpc1");

                var service = new PullPullAuthorizationMapService(provider);

                Assert.False(service.IsPullRequestAuthorized(pullRequest), "PullRequest should not be allowed since PullRequest is not signed");
            }
        }

        private static X509Certificate2 GetSigningCertificate()
        {
            var cert = new X509Certificate2(Properties.Resources.holodeck_partya_certificate,
                                            Properties.Resources.certificate_password, X509KeyStorageFlags.Exportable);

            Assert.NotNull(cert.PrivateKey);

            return cert;
        }

        private static AS4Message SignAS4MessageWithCertificate(AS4Message message, X509Certificate2 certificate)
        {
            return AS4MessageUtils.SignWithCertificate(message, certificate);
        }

        private static AS4Message CreatePullRequest(string mpc)
        {
            PullRequest pr = new PullRequest(mpc);

            return AS4Message.Create(pr, null);
        }
    }
}
