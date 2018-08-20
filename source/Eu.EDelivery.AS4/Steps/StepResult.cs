using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps
{
    /// <summary>
    /// Result given when a <see cref="IStep" /> is finished executing
    /// </summary>
    public class StepResult
    {
        private StepResult(bool succeeded)
        {
            Succeeded = succeeded;
        }

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
            return new StepResult(Succeeded) {MessagingContext = MessagingContext, CanProceed = false};
        }

        /// <summary>
        /// Return a Failed <see cref="StepResult" />.
        /// </summary>
        /// <param name="context">The <see cref="MessagingContext"/>.</param>
        /// <returns></returns>
        public static StepResult Failed(MessagingContext context)
        {
            return new StepResult(succeeded: false) {MessagingContext = context, CanProceed = false};
        }

        /// <summary>
        /// Return a Successful <see cref="StepResult" />
        /// </summary>
        /// <param name="context">The <see cref="MessagingContext"/>.</param>
        /// <returns></returns>
        public static StepResult Success(MessagingContext context)
        {
            return new StepResult(succeeded: true) {MessagingContext = context, CanProceed = true};
        }

        /// <summary>
        /// Return a Successful <see cref="StepResult" />
        /// </summary>
        /// <param name="context">The <see cref="MessagingContext"/>.</param>
        /// <returns></returns>
        public static Task<StepResult> SuccessAsync(MessagingContext context)
        {
            return Task.FromResult(Success(context));
        }

        /// <summary>
        /// Return a Failed <see cref="StepResult" />.
        /// </summary>
        /// <param name="context">The <see cref="MessagingContext"/>.</param>
        /// <returns></returns>
        public static Task<StepResult> FailedAsync(MessagingContext context)
        {
            return Task.FromResult(Failed(context));
        }
    }
}