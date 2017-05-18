using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Repositories
{
    internal class StubMessageBodyPersister : IAS4MessageBodyPersister
    {
        internal static StubMessageBodyPersister Default => new StubMessageBodyPersister();

        /// <summary>
        /// Saves a given <see cref="AS4Message"/> to a given location.
        /// </summary>
        /// <param name="message">The message to save.</param>
        /// <param name="cancellationToken">The Cancellation.</param>
        /// <returns>Location where the <paramref name="message"/> is saved.</returns>
        public Task<string> SaveAS4MessageAsync(AS4Message message, CancellationToken cancellationToken)
        {
            return Task.FromResult(string.Empty);
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
            return Task.CompletedTask;
        }
    }

    public class StubMessageBodyPersistorFacts
    {
        [Fact]
        public void UpdatesDirectly()
        {
            // Act
            Task updateTask = StubMessageBodyPersister.Default.UpdateAS4MessageAsync(null, null, CancellationToken.None);

            // Assert
            Assert.True(updateTask.IsCompleted);
        }
    }
}
