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

        /// <summary>
        /// Gets the included <see cref="AS4Exception"/> occurred during the step execution.
        /// </summary>
        public AS4Exception Exception { get; private set; }

        /// <summary>
        /// Gets the included <see cref="InternalMessage"/> send throughout the step execution.
        /// </summary>
        public InternalMessage InternalMessage { get; private set; }
        
        /// <summary>
        /// Gets a value indicating whether the next steps must be executed.
        /// </summary>
        public bool CanProceed { get; private set; }

        /// <summary>
        /// Promote the <see cref="StepResult"/> to stop the execution.
        /// </summary>
        /// <returns></returns>
        public StepResult AndStopExecution()
        {
            return new StepResult {InternalMessage = InternalMessage, Exception = Exception, CanProceed = false};
        }

        /// <summary>
        /// Promote the <see cref="StepResult"/> to stop the execution.
        /// </summary>
        /// <returns></returns>
        public Task<StepResult> AndStopExecutionAsync()
        {
            return Task.FromResult(AndStopExecution());
        }

        /// <summary>
        /// Return a Failed <see cref="StepResult" />.
        /// </summary>
        /// <param name="exception">Included <see cref="AS4Exception" /></param>
        /// <returns></returns>
        public static StepResult Failed(AS4Exception exception)
        {
            return new StepResult {Exception = exception};
        }

        /// <summary>
        /// Return a Failed <see cref="StepResult"/>.
        /// </summary>
        /// <param name="exception">Included <see cref="AS4Exception"/>.</param>
        /// <param name="internalMessage">Included failed <see cref="InternalMessage"/>.</param>
        /// <returns></returns>
        public static StepResult Failed(AS4Exception exception, InternalMessage internalMessage)
        {
            return new StepResult {Exception = exception, InternalMessage = internalMessage};
        }

        /// <summary>
        /// Return a Successful <see cref="StepResult" />
        /// </summary>
        /// <param name="message">Included <see cref="InternalMessage" />.</param>
        /// <returns></returns>
        public static StepResult Success(InternalMessage message)
        {
            return new StepResult {InternalMessage = message};
        }

        /// <summary>
        /// Return a Successful <see cref="StepResult" />
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static Task<StepResult> SuccessAsync(InternalMessage message)
        {
            return Task.FromResult(Success(message));
        }
    }
}