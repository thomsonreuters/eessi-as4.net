using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Transformers
{
    /// <summary>
    /// Contract to transform from a non-canonical format to a known canonical format inside the AS4 component.
    /// </summary>
    public interface ITransformer
    {
        /// <summary>
        /// Transform a given <see cref="ReceivedMessage"/> to a Canonical <see cref="InternalMessage"/> instance.
        /// </summary>
        /// <param name="message">Given message to transform.</param>
        /// <param name="cancellationToken">Cancellation which stops the transforming.</param>
        /// <returns></returns>
        Task<InternalMessage> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken);
    }
}