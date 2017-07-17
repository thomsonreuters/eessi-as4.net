namespace Eu.EDelivery.AS4.Fe
{
    /// <summary>
    /// To be returned to the client to indicate an error.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }
        /// <summary>
        /// Gets or sets the exception.
        /// This contains the stack trace if enabled
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public string Exception { get; set; }
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }
    }
}