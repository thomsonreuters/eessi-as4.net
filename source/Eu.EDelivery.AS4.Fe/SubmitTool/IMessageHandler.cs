using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Fe.SubmitTool
{
    /// <summary>
    /// Message handler interface
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.SubmitTool.IHandler" />
    public interface IMessageHandler : IHandler
    {
        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="toLocation">To location.</param>
        /// <returns></returns>
        Task Handle(SubmitMessage message, string toLocation);
    }
}