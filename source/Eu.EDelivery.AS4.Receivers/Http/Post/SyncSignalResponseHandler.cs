using System;
using System.Net;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using log4net;

namespace Eu.EDelivery.AS4.Receivers.Http.Post
{
    /// <summary>
    /// HTTP POST handler to respond with a synchronous <see cref="SignalMessage"/>.
    /// </summary>
    internal class SyncSignalResponseHandler : IHttpPostHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

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

            return context.AS4Message != null && context.AS4Message.IsEmpty == false;
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

            HttpStatusCode statusCode = DetermineHttpCodeFrom(context);
            Logger.Debug($"Respond with {Config.Encode((int)statusCode)} {Config.Encode(statusCode)}: Receipt/Errors are responded sync");
            
            return HttpResult.FromAS4Message(statusCode, context.AS4Message);
        }

        private static HttpStatusCode DetermineHttpCodeFrom(MessagingContext agentResult)
        {
            if (agentResult.ReceivingPMode != null
                && agentResult.AS4Message?.PrimaryMessageUnit is Error)
            {
                int? errorHttpCode = agentResult.ReceivingPMode.ReplyHandling?.ErrorHandling?.ResponseHttpCode;
                if (errorHttpCode.HasValue
                    && Enum.IsDefined(typeof(HttpStatusCode), errorHttpCode))
                {
                    return (HttpStatusCode) errorHttpCode;
                }

                return HttpStatusCode.InternalServerError;
            }

            return HttpStatusCode.OK;
        }
    }
}