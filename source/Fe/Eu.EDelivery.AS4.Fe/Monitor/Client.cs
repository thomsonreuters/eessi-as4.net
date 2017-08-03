using Microsoft.AspNet.SignalR;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    /// <summary>
    /// Implementation of the IClient to send messages using SignalR
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Monitor.IClient" />
    public class Client : IClient
    {
        /// <summary>
        /// Send info log
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendInfo(string message)
        {
            GlobalHost.ConnectionManager.GetHubContext<SubmitToolMessageHub>().Clients.All.onMessage(new ClientMessage
            {
                Message = message
            });
        }

        /// <summary>
        /// Send log containing PMode
        /// </summary>
        /// <param name="pmode">The pmode.</param>
        public void SendPmode(string pmode)
        {
            GlobalHost.ConnectionManager.GetHubContext<SubmitToolMessageHub>().Clients.All.onMessage(new ClientMessage
            {
                Message = pmode,
                Type = LogType.Pmode
            });
        }

        /// <summary>
        /// Sendg log containing an AS4 message
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="id">The id of the AS4 message</param>
        public void SendAs4Message(string message, string id)
        {
            GlobalHost.ConnectionManager.GetHubContext<SubmitToolMessageHub>().Clients.All.onMessage(new ClientMessage
            {
                Message = message,
                Data = id,
                Type = LogType.Message
            });
        }

        /// <summary>
        /// Sends log containing error
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendError(string message)
        {
            GlobalHost.ConnectionManager.GetHubContext<SubmitToolMessageHub>().Clients.All.onMessage(new ClientMessage
            {
                Message = message,
                Type = LogType.Error
            });
        }
    }
}