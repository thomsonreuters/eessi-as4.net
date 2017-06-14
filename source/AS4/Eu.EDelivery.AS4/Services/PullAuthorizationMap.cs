using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Services
{
    public class PullAuthorizationMap : IAuthorizationMap
    {
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
            return true;
        }
    }

    public interface IAuthorizationMap
    {
        /// <summary>
        /// Determines whether [is pull request authorized] [the specified request].
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="certificate">The certificate.</param>
        /// <returns>
        ///   <c>true</c> if [is pull request authorized] [the specified request]; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        bool IsPullRequestAuthorized(PullRequest request, X509Certificate2 certificate);
    }
}
