using System;

namespace Eu.EDelivery.AS4.Exceptions
{
    [Serializable]
    public class InvalidMessageException : Exception
    {
        public InvalidMessageException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMessageException"/> class.
        /// </summary>
        public InvalidMessageException(string message)
            : base(message)
        {
        }

        public InvalidMessageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
