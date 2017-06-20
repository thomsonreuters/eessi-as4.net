using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Exceptions.Handlers
{
    public class EmptyExceptionHandler : IAgentExceptionHandler
    {
        /// <summary>
        /// Handles the transformation exception.
        /// </summary>
        /// <param name="contents">The contents.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public Task<MessagingContext> HandleTransformationException(Stream contents, Exception exception)
        {
            return Task.FromResult(new MessagingContext(exception));
        }

        /// <summary>
        /// Handles the execution exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task<MessagingContext> HandleExecutionException(Exception exception, MessagingContext context)
        {
            return Task.FromResult(new MessagingContext(exception));
        }

        /// <summary>
        /// Handles the error exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task<MessagingContext> HandleErrorException(Exception exception, MessagingContext context)
        {
            return Task.FromResult(new MessagingContext(exception));
        }
    }
}
