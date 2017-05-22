using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.UnitTests.Repositories
{
    internal class StubMessageBodyPersister : IAS4MessageBodyPersister
    {
        internal static StubMessageBodyPersister Default = new StubMessageBodyPersister();

        public Task<string> SaveAS4MessageAsync(AS4Message message, CancellationToken cancellationToken)
        {
            return Task.FromResult(string.Empty);
        }

        public Task UpdateAS4MessageAsync(string location, AS4Message message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
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
        public async Task<string> SaveAS4MessageAsync(string storeLocation, AS4Message message, CancellationToken cancellation)
        {
            throw new System.NotImplementedException();
        }
    }
}
