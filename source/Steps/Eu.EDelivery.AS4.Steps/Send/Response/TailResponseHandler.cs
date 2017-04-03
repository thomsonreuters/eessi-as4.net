using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Steps.Send.Response
{
    /// <summary>
    /// <see cref="IAS4ResponseHandler" /> implementation that can be used as the 'Tail' of the chain of
    /// <see cref="IAS4ResponseHandler" />.
    /// </summary>
    public class TailResponseHandler : IAS4ResponseHandler
    {
        /// <summary>
        /// Handle the given <paramref name="response" />, but delegate to the next handler if you can't.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public Task<StepResult> HandleResponse(IAS4Response response)
        {
            return StepResult.SuccessAsync(response.ResultedMessage);
        }
    }
}