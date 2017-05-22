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
    /// <see cref="IAS4MessageBodyRetriever"/> implementation to sabotage the loading of a <see cref="Stream"/> at a given location.
    /// </summary>
    public class SaboteurMessageBodyRetriever : IAS4MessageBodyPersister
    {
        /// <summary>
        /// Loads a <see cref="Stream"/> at a given stored <paramref name="location"/>.
        /// </summary>
        /// <param name="location">The location on which the <see cref="Stream"/> is stored.</param>
        /// <returns></returns>
        public Stream LoadAS4MessageStream(string location)
        {
            throw new SaboteurException("Sabotage the load of AS4 Messages");
        }

        /// <summary>
        /// Saves a given <see cref="AS4Message" /> to a given location.
        /// </summary>
        /// <param name="storeLocation">The store location.</param>
        /// <param name="message">The message to save.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>
        /// Location where the <paramref name="message" /> is saved.
        /// </returns>
        public Task<string> SaveAS4MessageAsync(string storeLocation, AS4Message message, CancellationToken cancellation)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Updates an existing AS4 Message body.
        /// </summary>
        /// <param name="location">The location where the existing AS4Message body can be found.</param>
        /// <param name="message">The message that should overwrite the existing messagebody.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task UpdateAS4MessageAsync(string location, AS4Message message, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        [Fact]
        public void FailsToLoad()
        {
            Assert.ThrowsAny<Exception>(() => new SaboteurMessageBodyRetriever().LoadAS4MessageStream(null));
        }

        [Fact]
        public async Task FailsToSave()
        {
            await Assert.ThrowsAnyAsync<Exception>(
                () => new SaboteurMessageBodyRetriever().SaveAS4MessageAsync(null, null, CancellationToken.None));
        }

        [Fact]
        public async Task FailsToUpdate()
        {
            await Assert.ThrowsAnyAsync<Exception>(
                () => new SaboteurMessageBodyRetriever().UpdateAS4MessageAsync(null, null, CancellationToken.None));
        }
    }
}
