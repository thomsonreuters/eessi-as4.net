using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.UnitTests.Exceptions.Handlers
{
    public class SpyAgentExceptionHandler : IAgentExceptionHandler
    {
        public bool HandledTransformationException { get; private set; }
        public bool HandledExecutionException { get; private set; }
        public bool HandledErrorException { get; private set; }

        /// <summary>
        /// Handles the transformation exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageToTransform">The contents.</param>
        /// <returns></returns>
        public Task<MessagingContext> HandleTransformationException(Exception exception, ReceivedMessage messageToTransform)
        {
            HandledTransformationException = true;
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
            HandledExecutionException = true;
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
            HandledErrorException = true;
            return Task.FromResult(new MessagingContext(exception));
        }
    }
}
