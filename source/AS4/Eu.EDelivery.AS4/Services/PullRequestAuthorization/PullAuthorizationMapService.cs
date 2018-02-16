using System;
using System.Linq;
using System.Threading;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Services.PullRequestAuthorization
{
    public class PullAuthorizationMapService : IPullAuthorizationMapService
    {
        private readonly IPullAuthorizationMapProvider _mapProvider;
        private readonly ICertificateRepository _certificateRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullAuthorizationMapService"/> class.
        /// </summary>
        public PullAuthorizationMapService(IPullAuthorizationMapProvider pullAuthorizationMapProvider) :
            this(pullAuthorizationMapProvider, Registry.Instance.CertificateRepository)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PullAuthorizationMapService"/> class.
        /// </summary>
        public PullAuthorizationMapService(IPullAuthorizationMapProvider pullAuthorizationMapProvider,
                                           ICertificateRepository certificateRepository)
        {
            _mapProvider = pullAuthorizationMapProvider;
            _certificateRepository = certificateRepository;
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

            var certificateThumbPrint = RetrieveSigningCertificateThumbPrint(pullRequestMessage, _certificateRepository);

            var authorizationEntriesForCertificate =
                authorizationEntries.Where(a => StringComparer.OrdinalIgnoreCase.Equals(a.CertificateThumbprint, certificateThumbPrint));

            return authorizationEntriesForCertificate.Any() && authorizationEntriesForCertificate.All(a => a.Allowed);
        }

        private static string RetrieveSigningCertificateThumbPrint(AS4Message as4Message, ICertificateRepository certificateRepository)
        {
            var token =
                SecurityTokenReferenceProvider.Get(as4Message.EnvelopeDocument ?? AS4XmlSerializer.ToSoapEnvelopeDocument(as4Message, CancellationToken.None),
                                                   SecurityTokenType.Signing,
                                                   certificateRepository);

            return token.Certificate.Thumbprint;
        }
    }
}
