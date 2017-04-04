using System.Threading.Tasks;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send.Response;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send.Response
{
    /// <summary>
    /// <see cref="IAS4ResponseHandler"/> implementation to sink all the incomining responses.
    /// </summary>
    public class StubAS4ResponseHandler : IAS4ResponseHandler
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
