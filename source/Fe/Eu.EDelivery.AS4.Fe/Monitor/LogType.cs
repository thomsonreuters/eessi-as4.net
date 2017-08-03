namespace Eu.EDelivery.AS4.Fe.Monitor
{
    /// <summary>
    /// Identify different types of log messages
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// Contains information
        /// </summary>
        Info = 0,
        /// <summary>
        /// Contains an error message
        /// </summary>
        Error,
        /// <summary>
        /// Uploading a file
        /// </summary>
        Upload,
        /// <summary>
        /// Indicate that the submit tool is finished
        /// </summary>
        Done,
        /// <summary>
        /// It contains a PMode
        /// </summary>
        Pmode,
        /// <summary>
        /// It is an AS4 message
        /// </summary>
        Message
    }
}