using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps.Receive;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how a signed PullRequest signal-message is verified
    /// </summary>
    /// <seealso cref="IStep" />
    public class PullVerifySignatureStep : IStep
    {
        private readonly IStep _verificationStep = new VerifySignatureAS4MessageStep();

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext"/>.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            StepResult result = StepResult.Success(messagingContext);

            try
            {
                await _verificationStep.ExecuteAsync(messagingContext, cancellationToken);
                return result;
            }
            catch (Exception)
            {
                throw PullRequestValidationException.InvalidSignature(messagingContext.AS4Message.GetPrimaryMessageId());
            }
        }
    }
}
