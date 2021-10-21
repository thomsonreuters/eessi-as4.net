using System;
using System.Net;
using System.Security;
using System.Text;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using log4net;

namespace Eu.EDelivery.AS4.Receivers.Http.Post
{
    /// <summary>
    /// HTTP POST handler to correctly return a response for both unhandled Errors and Exceptions.
    /// </summary>
    internal class ExceptionPostHandler : IHttpPostHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

        /// <summary>
        /// Determines if the resulted context can be handled by this instance.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// 
        public bool CanHandle(MessagingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return
                context.Exception != null
                || context.ErrorResult != null
                && (context.AS4Message?.IsEmpty ?? true);
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

            HttpStatusCode statusCode = DetermineStatusCode(context.Exception);
            const string errorMessage = "something went wrong while processing the request";

            Logger.Error($"Respond with {Config.Encode((int) statusCode)} {Config.Encode(statusCode)} {Config.Encode(errorMessage)}");
            return HttpResult.FromBytes(
                statusCode,
                Encoding.UTF8.GetBytes(errorMessage),
                "text/plain");
        }

        private static HttpStatusCode DetermineStatusCode(Exception exception)
        {
            switch (exception)
            {
                case SecurityException _:
                    return HttpStatusCode.Forbidden;
                case InvalidMessageException _:
                    return HttpStatusCode.BadRequest;
                default:
                    return HttpStatusCode.InternalServerError;
            }
        }
    }
}