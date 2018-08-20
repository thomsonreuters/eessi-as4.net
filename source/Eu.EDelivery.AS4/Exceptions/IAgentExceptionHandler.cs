using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Exceptions
{
    /// <summary>
    /// This interface defines the contract of the future classes that will be responsible for handling exceptions that are thrown in the Agent.
    /// </summary>
    public interface IAgentExceptionHandler
    {
        /// <summary>
        /// Handles the transformation exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageToTransform">The contents.</param>
        /// <returns></returns>
        Task<MessagingContext> HandleTransformationException(Exception exception, ReceivedMessage messageToTransform);

        /// <summary>
        /// Handles the execution exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        Task<MessagingContext> HandleExecutionException(Exception exception, MessagingContext context);

        /// <summary>
        /// Handles the error exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        Task<MessagingContext> HandleErrorException(Exception exception, MessagingContext context);
    }
}