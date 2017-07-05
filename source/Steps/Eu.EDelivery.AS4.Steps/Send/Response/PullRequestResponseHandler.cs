using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps.Send.Response
{
    /// <summary>
    /// <see cref="IAS4ResponseHandler"/> implementation to handle the response for a Pull Request.
    /// </summary>
    internal sealed class PullRequestResponseHandler : IAS4ResponseHandler
    {
        private readonly IAS4ResponseHandler _nextHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestResponseHandler"/> class.
        /// </summary>
        /// <param name="nextHandler">The next Handler.</param>
        public PullRequestResponseHandler(IAS4ResponseHandler nextHandler)
        {
            _nextHandler = nextHandler;
        }

        /// <summary>
        /// Handle the given <paramref name="response" />, but delegate to the next handler if you can't.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task<StepResult> HandleResponse(IAS4Response response)
        {

            bool isOriginatedFromPullRequest = (response.ReceivedAS4Message.PrimarySignalMessage as Error)?.IsWarningForEmptyPullRequest == true;
            bool isRequestBeingSendAPullRequest = response.OriginalRequest.AS4Message?.IsPullRequest == true;

            ////response.ResultedMessage.AS4Message.Mep =
            ////    isRequestBeingSendAPullRequest
            ////        ? MessageExchangePattern.Pull
            ////        : MessageExchangePattern.Push;

            if (isOriginatedFromPullRequest)
            {
                return StepResult.Success(new MessagingContext(response.ReceivedAS4Message, MessagingContextMode.Send)).AndStopExecution();
            }

            return await _nextHandler.HandleResponse(response);
        }
    }
}