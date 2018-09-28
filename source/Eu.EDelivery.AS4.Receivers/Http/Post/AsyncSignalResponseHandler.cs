using System.Net;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;

namespace Eu.EDelivery.AS4.Receivers.Http.Post
{
    internal class AsyncSignalResponseHandler : IHttpPostHandler
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Determines if the resulted context can be handled by this instance.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool CanHandle(MessagingContext context)
        {
            return context.ReceivingPMode != null
                   && context.ReceivingPMode.ReplyHandling.ReplyPattern == ReplyPattern.Callback;
        }

        /// <summary>
        /// Handles the resulted context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public HttpResult Handle(MessagingContext context)
        {
            Logger.Debug("Respond with 202 Accepted: Receipt/Errors are responded async");
            return HttpResult.Empty(HttpStatusCode.Accepted);
        }
    }
}