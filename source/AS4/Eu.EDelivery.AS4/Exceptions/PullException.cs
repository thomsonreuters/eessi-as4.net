using System;

namespace Eu.EDelivery.AS4.Exceptions
{
    public class PullException : AS4Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PullException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        private PullException(string message) : base(message) {}

        /// <summary>
        /// Invalids the signature.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <returns></returns>
        public static Exception InvalidSignature(string messageId)
        {
            var exception = new PullException("Signature verifycation failed") {ErrorCode = ErrorCode.Ebms0101};
            exception.AddMessageId(messageId);

            return exception;
        }
    }
}