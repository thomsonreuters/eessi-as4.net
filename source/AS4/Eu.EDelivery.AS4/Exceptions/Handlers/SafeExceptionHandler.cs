using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Exceptions.Handlers
{
    public class SafeExceptionHandler : IAgentExceptionHandler
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IAgentExceptionHandler _innerHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeExceptionHandler" /> class.
        /// </summary>
        /// <param name="handler">The handler.</param>
        public SafeExceptionHandler(IAgentExceptionHandler handler)
        {
            _innerHandler = handler;
        }

        /// <summary>
        /// Handles the transformation exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="contents">The contents.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleTransformationException(Exception exception, Stream contents)
        {
            return await TryHandling(() => _innerHandler.HandleTransformationException(exception, contents));
        }

        /// <summary>
        /// Handles the execution exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleExecutionException(Exception exception, MessagingContext context)
        {
            return await TryHandling(() => _innerHandler.HandleExecutionException(exception, context));
        }

        /// <summary>
        /// Handles the error exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleErrorException(Exception exception, MessagingContext context)
        {
            return await TryHandling(() => _innerHandler.HandleErrorException(exception, context));
        }

        private async Task<MessagingContext> TryHandling(Func<Task<MessagingContext>> actionToTry)
        {
            try
            {
                return await actionToTry();
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                return new MessagingContext(exception);
            }
        }
    }
}
