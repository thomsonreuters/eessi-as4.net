using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Serialization
{
    /// <summary>
    /// Contract of the SOAP/MIME serializers to make the <see cref="AS4Message"/> transferable.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Synchronously serializes the given <see cref="AS4Message"/> to a given <paramref name="output"/> stream.
        /// </summary>
        /// <param name="message">The message to serialize.</param>
        /// <param name="output">The destination stream to where the message should be written.</param>
        /// <param name="cancellation">The token to control the cancellation of the serialization.</param>
        void Serialize(AS4Message message, Stream output, CancellationToken cancellation = default(CancellationToken));

        /// <summary>
        /// Asynchronously serializes the given <see cref="AS4Message"/> to a given <paramref name="output"/> stream.
        /// </summary>
        /// <param name="message">The message to serialize.</param>
        /// <param name="output">The destination stream to where the message should be written.</param>
        /// <param name="cancellation">The token to control the cancellation of the serialization.</param>
        Task SerializeAsync(AS4Message message, Stream output, CancellationToken cancellation = default(CancellationToken));

        /// <summary>
        /// Asynchronously deserializes the given <paramref name="input"/> stream to an <see cref="AS4Message"/> model.
        /// </summary>
        /// <param name="input">The source stream from where the message should be read.</param>
        /// <param name="contentType">The content type required to correctly deserialize the message into different MIME parts.</param>
        /// <param name="cancellation">The token to control the cancellation of the deserialization.</param>
        Task<AS4Message> DeserializeAsync(Stream input, string contentType, CancellationToken cancellation = default(CancellationToken));
    }
}