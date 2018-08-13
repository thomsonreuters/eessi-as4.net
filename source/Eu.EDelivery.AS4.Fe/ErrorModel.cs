namespace Eu.EDelivery.AS4.Fe
{
    /// <summary>
    /// Model representing an error
    /// </summary>
    public class ErrorModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is error.
        /// </summary>
        public bool IsError { get; set; }
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }
        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public string Exception { get; set; }
    }
}