using System;
using System.Linq;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Services.PullRequestAuthorization
{
    public class PullPullAuthorizationMapService : IPullAuthorizationMapService
    {
        private readonly IPullAuthorizationMapProvider _mapProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullPullAuthorizationMapService"/> class.
        /// </summary>
        public PullPullAuthorizationMapService(IPullAuthorizationMapProvider pullAuthorizationMapProvider)
        {
            _mapProvider = pullAuthorizationMapProvider;
        }

        /// <summary>
        /// Determines whether a pull-request is authorized
        /// </summary>
        /// <param name="pullRequestMessage">An AS4 Message for which the primary signal-message is a PullRequest.</param>
        /// <returns>True if the PullRequest is allowed to be processed.</returns>
        public bool IsPullRequestAuthorized(AS4Message pullRequestMessage)
        {
            if (pullRequestMessage == null)
            {
                throw new ArgumentNullException(nameof(pullRequestMessage));
            }

            if (pullRequestMessage.IsPullRequest == false)
            {
                throw new InvalidMessageException("The AS4 Message is not a PullRequest message");
            }

            var mpc = ((PullRequest)pullRequestMessage.PrimarySignalMessage).Mpc;

            var authorizationEntries = _mapProvider.RetrievePullRequestAuthorizationEntriesForMpc(mpc);

            if (authorizationEntries == null || authorizationEntries.Any() == false)
            {
                return true;
            }

            if (pullRequestMessage.SecurityHeader.IsSigned == false && authorizationEntries.Any())
            {
                return false;
            }

            var certificateThumbPrint = RetrieveSigningCertificateThumbPrint(pullRequestMessage);

            var authorizationEntriesForCertificate =
                authorizationEntries.Where(a => StringComparer.OrdinalIgnoreCase.Equals(a.CertificateThumbprint, certificateThumbPrint));

            return authorizationEntriesForCertificate.Any() && authorizationEntriesForCertificate.All(a => a.Allowed);
        }

        private static string RetrieveSigningCertificateThumbPrint(AS4Message as4Message)
        {
            return as4Message.SecurityHeader.SigningCertificate.Thumbprint;
        }
    }
}
