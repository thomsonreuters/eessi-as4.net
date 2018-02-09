using System.ComponentModel;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Services.PullRequestAuthorization;

namespace Eu.EDelivery.AS4.Steps.Send
{
    [Description("Verifies if the received PullRequest is authorized.")]
    [Info("Verify pull request authorization")]
    public class VerifyPullRequestAuthorizationStep : IStep
    {
        private readonly IPullAuthorizationMapProvider _pullAuthorizationMapProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="VerifyPullRequestAuthorizationStep"/> class.
        /// </summary>
        public VerifyPullRequestAuthorizationStep() : this(Config.Instance.PullRequestAuthorizationMapProvider) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerifyPullRequestAuthorizationStep" /> class.
        /// </summary>
        ///<param name="pullAuthorizationMapProvider">The IAuthorizationMapProvider instance that must be used</param>
        public VerifyPullRequestAuthorizationStep(IPullAuthorizationMapProvider pullAuthorizationMapProvider)
        {
            _pullAuthorizationMapProvider = pullAuthorizationMapProvider;
        }

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext" />.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            var as4Message = messagingContext.AS4Message;

            var authorizationMap = new PullAuthorizationMapService(_pullAuthorizationMapProvider);

            if (authorizationMap.IsPullRequestAuthorized(as4Message))
            {
                return StepResult.SuccessAsync(messagingContext);
            }

            throw new SecurityException($"The PullRequest for this MPC is not authorized.");
        }
    }
}