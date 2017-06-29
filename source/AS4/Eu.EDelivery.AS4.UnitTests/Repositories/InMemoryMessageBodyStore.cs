using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.UnitTests.Repositories
{
    /// <summary>
    /// In-Memory Implementation to store the <see cref="AS4Message"/> instances.
    /// </summary>
    /// <seealso cref="IAS4MessageBodyStore" />
    public class InMemoryMessageBodyStore : IAS4MessageBodyStore
    {
        private static readonly IAS4MessageBodyStore DefaultInstance = new InMemoryMessageBodyStore();
        private AS4Message _message;

        public static IAS4MessageBodyStore Default => DefaultInstance;

        /// <summary>
        /// Gets the stored message location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public Task<string> GetMessageLocation(string location, AS4Message message)
        {
            return Task.FromResult(string.Empty);
        }

        /// <summary>
        /// Loads a <see cref="Stream" /> at a given stored <paramref name="location" />.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public async Task<Stream> LoadMessagesBody(string location)
        {
            var messageStream = new MemoryStream();

            var serializer = new SoapEnvelopeSerializer();
            await serializer.SerializeAsync(_message, messageStream, CancellationToken.None);
            messageStream.Position = 0;

            return messageStream;
        }

        /// <summary>
        /// Saves a given <see cref="AS4Message" /> to a given location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="message">The message to save.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>
        /// Location where the <paramref name="message" /> is saved.
        /// </returns>
        public Task<string> SaveAS4MessageAsync(string location, AS4Message message, CancellationToken cancellation)
        {
            _message = message;

            return Task.FromResult("not empty location");
        }

        /// <summary>
        /// Updates an existing AS4 Message body.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="message">The message that should overwrite the existing messagebody.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task UpdateAS4MessageAsync(string location, AS4Message message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
