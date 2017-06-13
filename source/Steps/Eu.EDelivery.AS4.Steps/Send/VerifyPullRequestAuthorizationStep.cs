using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Services;

namespace Eu.EDelivery.AS4.Steps.Send
{
    public class VerifyPullRequestAuthorizationStep : IStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VerifyPullRequestAuthorizationStep" /> class.
        /// </summary>
        /// <param name="authorizationMap">The authorization map.</param>
        public VerifyPullRequestAuthorizationStep(IAuthorizationMap authorizationMap) {}

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext" />.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            return StepResult.SuccessAsync(messagingContext);
        }
    }
}