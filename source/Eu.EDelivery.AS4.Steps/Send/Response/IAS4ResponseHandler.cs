using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Steps.Send.Response
{
    /// <summary>
    /// Contract for a chain of handlers that handle the <see cref="AS4Response" />.
    /// </summary>
    internal interface IAS4ResponseHandler
    {
        /// <summary>
        /// Handle the given <paramref name="response" />, but delegate to the next handler if you can't.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        Task<StepResult> HandleResponse(IAS4Response response);
    }
}