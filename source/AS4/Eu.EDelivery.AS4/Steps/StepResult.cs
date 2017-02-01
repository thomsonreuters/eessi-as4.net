using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps
{
    /// <summary>
    /// Result given when a <see cref="IStep" /> is finished executing
    /// </summary>
    public class StepResult
    {

        public AS4Exception Exception { get; private set; }        
        public InternalMessage InternalMessage { get; private set; }

        private StepResult()
        {
        }

        /// <summary>
        /// Return a Successful <see cref="StepResult" />
        /// </summary>
        /// <param name="message">Included <see cref="AS4Message" /></param>
        /// <returns></returns>
        public static StepResult Success(InternalMessage message)
        {
            return new StepResult()
            {
                InternalMessage = message
            };
        }

        public static Task<StepResult> SuccessAsync(InternalMessage message)
        {
            return Task.FromResult(Success(message));
        }

        /// <summary>
        /// Return a Failed <see cref="StepResult" />
        /// </summary>
        /// <param name="exception">Included <see cref="System.Exception" /></param>
        /// <returns></returns>
        public static StepResult Failed(AS4Exception exception)
        {
            return new StepResult()
            {
                Exception = exception
            };
        }

        public static StepResult Failed(AS4Exception exception, InternalMessage internalMessage)
        {
            return new StepResult()
            {
                Exception = exception,
                InternalMessage = internalMessage
            };
        }
    }
}