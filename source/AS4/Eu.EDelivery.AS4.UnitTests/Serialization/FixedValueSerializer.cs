using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.UnitTests.Serialization
{
    /// <summary>
    /// <see cref="ISerializer"/> implementation to 'Stub' the serialization process.
    /// </summary>
    public class FixedValueSerializer : ISerializer
    {
        private readonly AS4Message _fixedMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedValueSerializer"/> class.
        /// </summary>
        public FixedValueSerializer(AS4Message as4Message)
        {
            _fixedMessage = as4Message;
        }

        public Task SerializeAsync(AS4Message message, Stream stream, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public void Serialize(AS4Message message, Stream stream, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<AS4Message> DeserializeAsync(Stream inputStream, string contentType, CancellationToken cancellationToken)
        {
            return Task.FromResult(_fixedMessage);
        }
    }
}
