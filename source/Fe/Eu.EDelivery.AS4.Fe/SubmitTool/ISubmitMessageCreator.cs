using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Fe.SubmitTool
{
    /// <summary>
    /// Interface to be implemented to make a message creator for the submit tool.
    /// </summary>
    public interface ISubmitMessageCreator
    {
        /// <summary>
        /// Submit a message
        /// </summary>
        /// <param name="submitInfo">The submit information.</param>
        /// <returns></returns>
        Task CreateSubmitMessages(MessagePayload submitInfo);

        /// <summary>
        /// Simulates the specified submit information.
        /// </summary>
        /// <param name="submitInfo">The submit information.</param>
        /// <returns>XML string representing the AS4Message</returns>
        /// <exception cref="BusinessException">Exception thrown to indicate that the pmode could not be found.</exception>
        Task<string> Simulate(MessagePayload submitInfo);
    }
}