using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send.Response
{
    /// <summary>
    /// <see cref="IAS4ResponseHandler"/> implementation to handle the response for a Pull Request.
    /// </summary>
    internal sealed class PullRequestResponseHandler : IAS4ResponseHandler
    {
        private readonly Func<DatastoreContext> _createContext;
        private readonly IAS4ResponseHandler _nextHandler;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        internal PullRequestResponseHandler(IAS4ResponseHandler nextHandler) 
            : this(Registry.Instance.CreateDatastoreContext, nextHandler) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestResponseHandler"/> class.
        /// </summary>
        public PullRequestResponseHandler(
            Func<DatastoreContext> createContext,
            IAS4ResponseHandler nextHandler)
        {
            if (createContext == null)
            {
                throw new ArgumentNullException(nameof(createContext));
            }

            if (nextHandler == null)
            {
                throw new ArgumentNullException(nameof(nextHandler));
            }

            _createContext = createContext;
            _nextHandler = nextHandler;
        }

        /// <summary>
        /// Handle the given <paramref name="response" />, but delegate to the next handler if you can't.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task<StepResult> HandleResponse(IAS4Response response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            MessagingContext request = response.OriginalRequest;
            if (request?.AS4Message?.IsPullRequest == true)
            {
                bool pullRequestWasPiggyBacked = 
                    request.AS4Message.SignalMessages.Any(s => !(s is PullRequest));

                if (response.StatusCode != HttpStatusCode.Accepted
                    && response.StatusCode != HttpStatusCode.OK
                    && pullRequestWasPiggyBacked)
                {
                    Logger.Debug("Reset PiggyBacked SignalMessage(s) for the next PullRequest because it was not correctly send to the sender MSH");
                    using (DatastoreContext ctx = _createContext())
                    {
                        var service = new PiggyBackingService(ctx);
                        service.ResetToBePiggyBackedSignalMessages(request.AS4Message.SignalMessages);

                        await ctx.SaveChangesAsync().ConfigureAwait(false);
                    }
                }

                bool isEmptyChannelWarning = 
                    (response.ReceivedAS4Message?.FirstSignalMessage as Error)?.IsWarningForEmptyPullRequest == true;

                if (isEmptyChannelWarning)
                {
                    request.ModifyContext(response.ReceivedAS4Message, MessagingContextMode.Send);
                    return StepResult.Success(response.OriginalRequest).AndStopExecution();
                }
            }

            return await _nextHandler.HandleResponse(response);
        }
    }
}