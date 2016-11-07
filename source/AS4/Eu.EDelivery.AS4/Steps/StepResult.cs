using System.IO;
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
        private static StepResult _successResult, _failedResult;

        public AS4Exception Exception { get; set; }
        public Stream Result { get; set; }
        public InternalMessage InternalMessage { get; set; }

        /// <summary>
        /// Return a Successful <see cref="StepResult" />
        /// </summary>
        /// <param name="message">Included <see cref="AS4Message" /></param>
        /// <returns></returns>
        public static StepResult Success(InternalMessage message)
        {
            if (_successResult == null)
                _successResult = new StepResult();
            _successResult.InternalMessage = message;

            return _successResult;
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
            if (_failedResult == null)
                _failedResult = new StepResult();
            _failedResult.Exception = exception;

            return _failedResult;
        }

        public static StepResult Failed(AS4Exception exception, InternalMessage internalMessage)
        {
            if (_failedResult == null)
                _failedResult = new StepResult();
            _failedResult.Exception = exception;
            _failedResult.InternalMessage = internalMessage;

            return _failedResult;
        }
    }
}