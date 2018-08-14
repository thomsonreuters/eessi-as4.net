using System;
using System.IO;
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
            if (response.ReceivedAS4Message.IsEmpty)
            {
                if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    response.OriginalRequest.ModifyContext(response.ReceivedAS4Message, MessagingContextMode.Send);
                    return StepResult.Success(response.OriginalRequest).AndStopExecution();
                }

                Logger.Error($"Response with HTTP status: {response.StatusCode}");

                if (Logger.IsErrorEnabled)
                {
                    using (var r = new StreamReader(response.ReceivedStream.UnderlyingStream))
                    {
                        string content = await r.ReadToEndAsync();
                        if (!string.IsNullOrEmpty(content))
                        {
                            Logger.Error("Response with HTTP content: " + content);
                        }
                    }
                }

                response.OriginalRequest.ModifyContext(response.ReceivedStream, MessagingContextMode.Send);
                return StepResult.Failed(response.OriginalRequest).AndStopExecution();
            }

            return await _nextHandler.HandleResponse(response);
        }
    }
}