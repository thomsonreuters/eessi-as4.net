using System.ComponentModel;
using System.Security;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Services.PullRequestAuthorization;

namespace Eu.EDelivery.AS4.Steps.Send
{
    [Info("Verify pull request authorization")]
    [Description("Verifies if the received PullRequest is authorized")]
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
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            AS4Message as4Message = messagingContext.AS4Message;

            var authorizationMap = new PullAuthorizationMapService(_pullAuthorizationMapProvider);
            if (authorizationMap.IsPullRequestAuthorized(as4Message))
            {
                return StepResult.SuccessAsync(messagingContext);
            }

            string mpc = (as4Message.PrimaryMessageUnit as PullRequest)?.Mpc ?? string.Empty;
            throw new SecurityException(
                $"{messagingContext.LogTag} PullRequest for MPC {mpc} is not authorized. " + 
                "Either change the PullRequest MPC or add the MPC value to the authorization map");
        }
    }
}