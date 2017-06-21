using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Exceptions.Handlers
{
    internal class LogExceptionHandler : IAgentExceptionHandler
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Handles the transformation exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="contents">The contents.</param>
        /// <returns></returns>
        public Task<MessagingContext> HandleTransformationException(Exception exception, Stream contents)
        {
            return HandleException(exception);
        }

        /// <summary>
        /// Handles the execution exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task<MessagingContext> HandleExecutionException(Exception exception, MessagingContext context)
        {
            return HandleException(exception);
        }

        /// <summary>
        /// Handles the error exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task<MessagingContext> HandleErrorException(Exception exception, MessagingContext context)
        {
            return HandleException(exception);
        }

        private static Task<MessagingContext> HandleException(Exception exception)
        {
            Logger.Error(exception);
            return Task.FromResult(new MessagingContext(exception));
        }
    }
}
