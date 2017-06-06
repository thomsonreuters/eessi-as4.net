using System.Net;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send.Response
{
    /// <summary>
    /// <see cref="IAS4ResponseHandler"/> implementation to handle the response for a empty body.
    /// </summary>
    internal sealed class EmptyBodyResponseHandler : IAS4ResponseHandler
    {
        private readonly IAS4ResponseHandler _nextHandler;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyBodyResponseHandler"/> class.
        /// </summary>
        /// <param name="nextHandler">The next Handler.</param>
        public EmptyBodyResponseHandler(IAS4ResponseHandler nextHandler)
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
            if (response.ResultedMessage?.AS4Message.IsEmpty == true)
            {
                if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    return StepResult.Success(response.ResultedMessage).AndStopExecution();
                }
                else
                {
                    Logger.Error($"Response with HTTP status {response.StatusCode} received.");
                    
                    if (response.ResultedMessage?.Exception != null)
                    {
                        Logger.Error($"Additional information: {response.ResultedMessage.Exception.Message}");
                    }

                    return StepResult.Failed(response.ResultedMessage.Exception, response.ResultedMessage).AndStopExecution();
                }
            }

            return await _nextHandler.HandleResponse(response);
        }
    }
}