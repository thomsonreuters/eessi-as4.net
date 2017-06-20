using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Exceptions.Handlers
{
    [ExcludeFromCodeCoverage]
    public class MinderExceptionHandler : IAgentExceptionHandler
    {
        private readonly IAgentExceptionHandler _inboudHandler = new InboundExceptionHanlder();
        private readonly IAgentExceptionHandler _outboundHandler = new OutboundExceptionHandler();

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
        public async Task<MessagingContext> HandleExecutionException(Exception exception, MessagingContext context)
        {
            return await HandleMinderException(context, handler => handler.HandleExecutionException(exception, context));
        }

        /// <summary>
        /// Handles the error exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleErrorException(Exception exception, MessagingContext context)
        {
            return await HandleMinderException(context, handler => handler.HandleErrorException(exception, context));
        }

        private async Task<MessagingContext> HandleMinderException(MessagingContext context, Func<IAgentExceptionHandler, Task<MessagingContext>> handleException)
        {
            Func<MessagingContext, bool> isSubmitMessage = m => m.Mode == MessagingContextMode.Submit;

            return isSubmitMessage(context)
                       ? await handleException(_outboundHandler)
                       : await handleException(_inboudHandler);
        }
    }
}
