using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Transformers
{
    public interface ITransformer
    {
        Task<InternalMessage> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken);
    }
}