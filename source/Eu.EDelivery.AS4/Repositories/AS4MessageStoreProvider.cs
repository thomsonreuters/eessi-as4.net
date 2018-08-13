using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task<Stream> LoadMessageBodyAsync(string location)
        {
            return await For(location).LoadMessageBodyAsync(location);
        }

        /// <summary>
        /// Saves a given <see cref="AS4Message" /> to a given location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="message">The message to save.</param>
        /// <returns>
        /// Location where the <paramref name="message" /> is saved.
        /// </returns>
        public string SaveAS4Message(string location, AS4Message message)
        {
            return For(location).SaveAS4Message(location, message);
        }

        public async Task<string> SaveAS4MessageStreamAsync(string location, Stream as4MessageStream)
        {
            return await For(location).SaveAS4MessageStreamAsync(location, as4MessageStream);
        }

        /// <summary>
        /// Updates an existing AS4 Message body.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="message">The message that should overwrite the existing messagebody.</param>
        /// <returns></returns>
        public void UpdateAS4Message(string location, AS4Message message)
        {
            For(location).UpdateAS4Message(location, message);
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
                throw new KeyNotFoundException($"No registered {nameof(IAS4MessageBodyStore)} found for {key}");
            }

            return entry.Value;
        }
    }
}