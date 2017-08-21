using System;
using System.Collections.Generic;
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
    public class InMemoryMessageBodyStore : IAS4MessageBodyStore, IDisposable
    {
        private readonly Dictionary<string, Stream> _store = new Dictionary<string, Stream>();

        public static IAS4MessageBodyStore Default { get; } = new InMemoryMessageBodyStore();

        /// <summary>
        /// Loads a <see cref="Stream" /> at a given stored <paramref name="location" />.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public async Task<Stream> LoadMessageBodyAsync(string location)
        {
            if (_store.ContainsKey(location) == false)
            {
                throw new InvalidOperationException($"MessageBodyStore does not contain an entry for {location}");
            }

            var messageStream = _store[location];
            messageStream.Position = 0;

            return await Task.FromResult(messageStream);
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
        public async Task<string> SaveAS4MessageAsync(string location, AS4Message message, CancellationToken cancellation)
        {
            string id = Guid.NewGuid().ToString();

            var serializer = SerializerProvider.Default.Get(message.ContentType);
            var stream = new MemoryStream();
            await serializer.SerializeAsync(message, stream, cancellation);

            _store.Add(id, stream);

            return await Task.FromResult(id);
        }

        public Task<string> SaveAS4MessageStreamAsync(string location, Stream as4MessageStream, CancellationToken cancellation)
        {
            string locationId = Guid.NewGuid().ToString();

            _store.Add(locationId, as4MessageStream);

            return Task.FromResult(locationId);
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

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            foreach (var kvp in _store)
            {
                kvp.Value.Dispose();
            }
            _store.Clear();
        }
    }
}
