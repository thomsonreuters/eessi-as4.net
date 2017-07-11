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
    }
}