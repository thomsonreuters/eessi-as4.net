using Microsoft.AspNet.SignalR;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    /// <summary>
    /// SignalR messagehub used for communicating with the submit tool client(s)
    /// </summary>
    /// <seealso cref="Microsoft.AspNet.SignalR.Hub" />
    [Authorize]
    public class SubmitToolMessageHub : Hub
    {

    }
}