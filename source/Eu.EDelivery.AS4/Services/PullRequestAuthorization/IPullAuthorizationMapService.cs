using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Services.PullRequestAuthorization
{
    public interface IPullAuthorizationMapService
    {
        /// <summary>
        /// Determines whether a pull-request is authorized
        /// </summary>
        /// <param name="pullRequestMessage">An AS4 Message for which the primary signal-message is a PullRequest.</param>
        /// <returns>True if the PullRequest is allowed to be processed.</returns>
        bool IsPullRequestAuthorized(AS4Message pullRequestMessage);
    }
}