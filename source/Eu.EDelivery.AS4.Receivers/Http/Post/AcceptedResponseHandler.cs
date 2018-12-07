using System;
using System.Net;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Receivers.Http.Post
{
    internal class AcceptedResponseHandler : IHttpPostHandler
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Determines if the resulted context can be handled by this instance.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool CanHandle(MessagingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.Exception == null
                   && context.ErrorResult == null
                   && context.AS4Message != null 
                   && context.AS4Message.IsEmpty;
        }

        /// <summary>
        /// Handles the resulted context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public HttpResult Handle(MessagingContext context)
        {
            Logger.Debug("Respond with 202 Accepted: unknown reason");
            return HttpResult.Empty(HttpStatusCode.Accepted);
        }
    }
}
