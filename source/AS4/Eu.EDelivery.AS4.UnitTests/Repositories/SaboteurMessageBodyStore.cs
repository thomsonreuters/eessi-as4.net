using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.UnitTests.Strategies.Sender;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Repositories
{
    /// <summary>
    /// <see cref="IAS4MessageBodyStore" /> implementation to sabotage the loading of a <see cref="Stream" /> at a given
    /// location.
    /// </summary>
    public class SaboteurMessageBodyStore : IAS4MessageBodyStore
    {
        /// <summary>
        /// Loads a <see cref="T:System.IO.Stream" /> at a given stored <paramref name="location" />.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        /// <exception cref="SaboteurException">Sabotage the load of AS4 Messages</exception>
        public Task<Stream> LoadMessagesBody(string location)
        {
            throw new SaboteurException("Sabotage the load of AS4 Messages");
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
        /// <exception cref="NotImplementedException"></exception>
        public Task<string> SaveAS4MessageAsync(string location, AS4Message message, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates an existing AS4 Message body.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="message">The message that should overwrite the existing messagebody.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task UpdateAS4MessageAsync(string location, AS4Message message, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task FailsToLoad()
        {
            await Assert.ThrowsAnyAsync<Exception>(() => new SaboteurMessageBodyStore().LoadMessagesBody(null));
        }

        [Fact]
        public async Task FailsToSave()
        {
            await Assert.ThrowsAnyAsync<Exception>(
                () => new SaboteurMessageBodyStore().SaveAS4MessageAsync(null, null, CancellationToken.None));
        }

        [Fact]
        public async Task FailsToUpdate()
        {
            await Assert.ThrowsAnyAsync<Exception>(
                () => new SaboteurMessageBodyStore().UpdateAS4MessageAsync(null, null, CancellationToken.None));
        }
    }
}