using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send.Response;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send.Response
{
    /// <summary>
    /// <see cref="IAS4ResponseHandler"/> implementation to 'Spy' on the handling of the response.
    /// </summary>
    internal class SpyAS4ResponseHandler : IAS4ResponseHandler
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="IAS4ResponseHandler"/> is called to handle the response.
        /// </summary>
        public bool IsCalled { get; private set; }

        /// <summary>
        /// Handle the given <paramref name="response" />, but delegate to the next handler if you can't.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public Task<StepResult> HandleResponse(IAS4Response response)
        {
            IsCalled = true;

            return StepResult.SuccessAsync(new MessagingContext(response.ReceivedStream, MessagingContextMode.Send));
        }
    }
}
