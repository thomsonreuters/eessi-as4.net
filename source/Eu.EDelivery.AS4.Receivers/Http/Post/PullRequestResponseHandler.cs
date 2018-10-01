using System;
using System.Net;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Receivers.Http.Post
{
    internal class PullRequestResponseHandler : IHttpPostHandler
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

            return context.Mode == MessagingContextMode.Send && context.ReceivedMessage != null;
        }

        /// <summary>
        /// Handles the resulted context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public HttpResult Handle(MessagingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.ReceivedMessage == null)
            {
                throw new ArgumentNullException(nameof(context.ReceivedMessage));
            }

            Logger.Debug("Respond with 200 OK: AS4Message is result of pulling");

            // When we're sending as a puller, make sure that the message that has been received, 
            // is directly written to the stream.
            return HttpResult.FromStream(
                HttpStatusCode.OK,
                context.ReceivedMessage.UnderlyingStream,
                context.ReceivedMessage.ContentType);
        }
    }
}