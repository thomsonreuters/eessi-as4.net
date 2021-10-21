using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using log4net;

namespace Eu.EDelivery.AS4.Exceptions.Handlers
{
    internal class LogExceptionHandler : IAgentExceptionHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

        /// <summary>
        /// Handles the transformation exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageToTransform">The <see cref="ReceivedMessage"/> that must be transformed by the transformer.</param>
        /// <returns></returns>
        public Task<MessagingContext> HandleTransformationException(Exception exception, ReceivedMessage messageToTransform)
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
