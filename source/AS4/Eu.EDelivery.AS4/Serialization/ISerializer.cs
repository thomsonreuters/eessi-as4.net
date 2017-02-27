using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Serialization
{
    /// <summary>
    /// Serialize to RequestStream
    /// </summary>
    public interface ISerializer
    {
        // TODO: it would be better if Serialize returned the stream instead of accepting it as a parameter.
        void Serialize(AS4Message message, Stream stream, CancellationToken cancellationToken);
        Task<AS4Message> DeserializeAsync(Stream inputStream, string contentType, CancellationToken cancellationToken);
    }
}