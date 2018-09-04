using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps
{
    /// <summary>
    /// Result given when a <see cref="IStep" /> is finished executing
    /// </summary>
    public class StepResult
    {
        private StepResult(bool succeeded, bool canProceed, MessagingContext context)
        {
            Succeeded = succeeded;
            MessagingContext = context;
            CanProceed = canProceed;
        }

        /// <summary>
        /// Gets the included <see cref="MessagingContext"/> send throughout the step execution.
        /// </summary>
        public MessagingContext MessagingContext { get; }
        
        /// <summary>
        /// Gets a value indicating whether the next steps must be executed.
        /// </summary>
        public bool CanProceed { get; }

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
            return new StepResult(Succeeded, canProceed: false, context: MessagingContext);
        }

        /// <summary>
        /// Return a Failed <see cref="StepResult" />.
        /// </summary>
        /// <param name="context">The <see cref="MessagingContext"/>.</param>
        /// <returns></returns>
        public static StepResult Failed(MessagingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return new StepResult(succeeded: false, canProceed: false, context: context);
        }

        /// <summary>
        /// Return a Successful <see cref="StepResult" />
        /// </summary>
        /// <param name="context">The <see cref="MessagingContext"/>.</param>
        /// <returns></returns>
        public static StepResult Success(MessagingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return new StepResult(succeeded: true, canProceed: true, context: context);
        }

        /// <summary>
        /// Return a Successful <see cref="StepResult" />
        /// </summary>
        /// <param name="context">The <see cref="MessagingContext"/>.</param>
        /// <returns></returns>
        public static Task<StepResult> SuccessAsync(MessagingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Task.FromResult(Success(context));
        }

        /// <summary>
        /// Return a Failed <see cref="StepResult" />.
        /// </summary>
        /// <param name="context">The <see cref="MessagingContext"/>.</param>
        /// <returns></returns>
        public static Task<StepResult> FailedAsync(MessagingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Task.FromResult(Failed(context));
        }
    }
}