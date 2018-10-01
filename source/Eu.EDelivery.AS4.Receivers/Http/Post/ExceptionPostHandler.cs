using System;
using System.Net;
using System.Security;
using System.Text;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Receivers.Http.Post
{
    internal class ExceptionPostHandler : IHttpPostHandler
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

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

            string errorMessage =
                String.IsNullOrWhiteSpace(context.ErrorResult?.Description) == false
                    ? context.ErrorResult.Description
                    : context.Exception?.Message ?? string.Empty;

            Logger.Error($"Respond with {(int) statusCode} {statusCode} {(string.IsNullOrEmpty(errorMessage) ? String.Empty : errorMessage)}");
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