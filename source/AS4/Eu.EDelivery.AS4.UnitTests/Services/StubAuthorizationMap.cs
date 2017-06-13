using System;
using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Services;

namespace Eu.EDelivery.AS4.UnitTests.Services
{
    internal class StubAuthorizationMap : IAuthorizationMap
    {
        private readonly Func<PullRequest, X509Certificate2, bool> _implementation;

        /// <summary>
        /// Initializes a new instance of the <see cref="StubAuthorizationMap" /> class.
        /// </summary>
        /// <param name="implementation">The implementation.</param>
        public StubAuthorizationMap(Func<PullRequest, X509Certificate2, bool> implementation)
        {
            _implementation = implementation;
        }

        /// <summary>
        /// Determines whether [is pull request authorized] [the specified request].
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="certificate">The certificate.</param>
        /// <returns>
        ///   <c>true</c> if [is pull request authorized] [the specified request]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPullRequestAuthorized(PullRequest request, X509Certificate2 certificate)
        {
            return _implementation(request, certificate);
        }
    }
}
