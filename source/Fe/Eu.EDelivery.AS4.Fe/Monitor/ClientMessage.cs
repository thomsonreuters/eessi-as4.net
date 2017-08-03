namespace Eu.EDelivery.AS4.Fe.Monitor
{
    /// <summary>
    /// Object to hold a log message to send to the client
    /// </summary>
    public class ClientMessage
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public LogType Type { get; set; } = LogType.Info;
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public string Data { get; set; }
    }
}