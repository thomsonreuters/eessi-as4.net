using System;

namespace Eu.EDelivery.AS4.Fe
{
    /// <summary>
    /// Exception class to carry business exceptions
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class BusinessException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public BusinessException(string message) : base(message)
        {
        }
    }
}
