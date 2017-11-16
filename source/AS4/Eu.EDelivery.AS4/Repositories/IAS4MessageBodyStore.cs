using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Repositories
{
    public interface IAS4MessageBodyStore
    {
        /// <summary>
        /// Loads a <see cref="Stream" /> at a given stored <paramref name="location" />.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        Task<Stream> LoadMessageBodyAsync(string location);

        /// <summary>
        /// Saves a given <see cref="AS4Message" /> to a given location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="message">The message to save.</param>
        /// <returns>
        /// Location where the <paramref name="message" /> is saved.
        /// </returns>
        string SaveAS4Message(string location, AS4Message message);

        Task<string> SaveAS4MessageStreamAsync(string location, Stream as4MessageStream, CancellationToken cancellation);

        /// <summary>
        /// Updates an existing AS4 Message body.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="message">The message that should overwrite the existing messagebody.</param>        
        /// <returns></returns>
        void UpdateAS4Message(string location, AS4Message message);
    }
}