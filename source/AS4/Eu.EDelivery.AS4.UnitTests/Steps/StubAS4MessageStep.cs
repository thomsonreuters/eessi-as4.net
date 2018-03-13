using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;

namespace Eu.EDelivery.AS4.UnitTests.Steps
{
    internal class StubAS4MessageStep : IStep
    {
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            return StepResult.SuccessAsync(new MessagingContext(AS4Message.Empty, MessagingContextMode.Submit));
        }
    }
}