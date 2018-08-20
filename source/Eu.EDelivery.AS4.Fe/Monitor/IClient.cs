namespace Eu.EDelivery.AS4.Fe.Monitor
{
    /// <summary>
    /// Interface to be implemented to send Submit tool messages to the client
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// Send info log
        /// </summary>
        /// <param name="message">The message.</param>
        void SendInfo(string message);

        /// <summary>
        /// Send log containing PMode
        /// </summary>
        /// <param name="pmode">The pmode.</param>
        void SendPmode(string pmode);

        /// <summary>
        /// Sends log containing error
        /// </summary>
        /// <param name="message">The message.</param>
        void SendError(string message);

        /// <summary>
        /// Sendg log containing an AS4 message
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="id">The id of the AS4 message</param>
        void SendAs4Message(string message, string id);
    }
}