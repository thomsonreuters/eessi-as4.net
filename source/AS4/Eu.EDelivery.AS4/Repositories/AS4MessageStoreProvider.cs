using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Repositories
{
    public class MessageBodyStore : IAS4MessageBodyStore
    {
        private readonly IDictionary<Func<string, bool>, IAS4MessageBodyStore> _stores = 
            new Dictionary<Func<string, bool>, IAS4MessageBodyStore>();

        /// <summary>
        /// Accepts the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="persister">The persister.</param>
        public void Accept(Func<string, bool> condition, IAS4MessageBodyStore persister)
        {
            _stores[condition] = persister;
        }

        /// <summary>
        /// Loads a <see cref="Stream" /> at a given stored <paramref name="location" />.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public async Task<Stream> LoadMessagesBody(string location)
        {
            return await For(location).LoadMessagesBody(location);
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
            return await For(location).SaveAS4MessageAsync(location, message, cancellation);
        }

        /// <summary>
        /// Updates an existing AS4 Message body.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="message">The message that should overwrite the existing messagebody.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task UpdateAS4MessageAsync(string location, AS4Message message, CancellationToken cancellationToken)
        {
            await For(location).UpdateAS4MessageAsync(location, message, cancellationToken);
        }

        private IAS4MessageBodyStore For(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            KeyValuePair<Func<string, bool>, IAS4MessageBodyStore> entry = _stores.FirstOrDefault(c => c.Key(key));

            if (entry.Value == null)
            {
                throw new AS4Exception($"No registered '{nameof(IAS4MessageBodyStore)}' found for {key}");
            }

            return entry.Value;
        }

        private sealed class AS4MessageRepositoryEntry
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AS4MessageRepositoryEntry" /> class.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="persister">The persister.</param>
            public AS4MessageRepositoryEntry(
                Func<string, bool> condition,
                IAS4MessageBodyStore persister)
            {
                Condition = condition;
                Store = persister;
            }

            public IAS4MessageBodyStore Store { get; }

            public Func<string, bool> Condition { get; }
        }
    }
}