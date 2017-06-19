using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Transformers;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    public class StubSubmitTransformer : ITransformer
    {
        public Task<MessagingContext> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            return Task.FromResult(new MessagingContext(new SubmitMessage()));
        }
    }
}