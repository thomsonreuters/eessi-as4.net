using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps
{
    /// <summary>
    /// Result given when a <see cref="IStep" /> is finished executing
    /// </summary>
    public class StepResult
    {
        private StepResult()
        {
            CanProceed = true;
        }

        private StepResult(bool succeeded)
        {
            Succeeded = succeeded;
        }

        /// <summary>
        /// Gets the included <see cref="AS4Exception"/> occurred during the step execution.
        /// </summary>
        public AS4Exception Exception { get; private set; }

        /// <summary>
        /// Gets the error result.
        /// </summary>
        /// <value>The error result.</value>
        public ErrorResult ErrorResult { get; private set; }

        /// <summary>
        /// Gets the included <see cref="MessagingContext"/> send throughout the step execution.
        /// </summary>
        public MessagingContext MessagingContext { get; private set; }
        
        /// <summary>
        /// Gets a value indicating whether the next steps must be executed.
        /// </summary>
        public bool CanProceed { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [was succesful].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [was succesful]; otherwise, <c>false</c>.
        /// </value>
        public bool Succeeded { get; }

        /// <summary>
        /// Promote the <see cref="StepResult"/> to stop the execution.
        /// </summary>
        /// <returns></returns>
        public StepResult AndStopExecution()
        {
            return new StepResult(Succeeded) {MessagingContext = MessagingContext, Exception = Exception, CanProceed = false};
        }

        /// <summary>
        /// Return a Failed <see cref="StepResult" />.
        /// </summary>
        /// <param name="exception">Included <see cref="AS4Exception" /></param>
        /// <returns></returns>
        [Obsolete]
        public static StepResult Failed(AS4Exception exception)
        {
            return new StepResult(succeeded: false) {Exception = exception};
        }

        /// <summary>
        /// Return a Failed <see cref="StepResult"/>.
        /// </summary>
        /// <param name="exception">Included <see cref="AS4Exception"/>.</param>
        /// <param name="messagingContext">Included failed <see cref="MessagingContext"/>.</param>
        /// <returns></returns>
        [Obsolete]
        public static StepResult Failed(AS4Exception exception, MessagingContext messagingContext)
        {
            return new StepResult(succeeded: false) {Exception = exception, MessagingContext = messagingContext};
        }

        /// <summary>
        /// Return a Failed <see cref="StepResult" />.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static StepResult Failed(ErrorResult error, MessagingContext context)
        {
            return new StepResult(succeeded: false) {ErrorResult = error, MessagingContext = context};
        }
        
        /// <summary>
        /// Return a Successful <see cref="StepResult" />
        /// </summary>
        /// <param name="message">Included <see cref="MessagingContext" />.</param>
        /// <returns></returns>
        public static StepResult Success(MessagingContext message)
        {
            return new StepResult(succeeded: true) {MessagingContext = message, CanProceed = true};
        }

        /// <summary>
        /// Return a Successful <see cref="StepResult" />
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static Task<StepResult> SuccessAsync(MessagingContext message)
        {
            return Task.FromResult(Success(message));
        }
    }
}