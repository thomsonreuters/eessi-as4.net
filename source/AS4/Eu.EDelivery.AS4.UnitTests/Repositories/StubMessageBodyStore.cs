using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Repositories
{
    public class StubMessageBodyStore : IAS4MessageBodyStore
    {
        private readonly string _messageLocation;

        internal static StubMessageBodyStore Default => new StubMessageBodyStore();

        /// <summary>
        /// Initializes a new instance of the <see cref="StubMessageBodyStore" /> class.
        /// </summary>
        public StubMessageBodyStore() : this(string.Empty) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="StubMessageBodyStore"/> class.
        /// </summary>
        /// <param name="messageLocation">The message location.</param>
        public StubMessageBodyStore(string messageLocation)
        {
            _messageLocation = messageLocation;
        }

        /// <summary>
        /// Determines whether [is message already saved] [the specified location].
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public Task<string> GetMessageLocation(string location, AS4Message message)
        {
            return Task.FromResult(_messageLocation);
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
            return Task.FromResult(_messageLocation);
        }

        /// <summary>
        /// Loads a <see cref="Stream" /> at a given stored <paramref name="location" />.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public virtual Task<Stream> LoadMessagesBody(string location)
        {
            return Task.FromResult(Stream.Null);
        }
    }

    public class StubMessageBodyStoreFacts
    {
        [Fact]
        public void UpdatesComplete()
        {
            Assert.True(StubMessageBodyStore.Default.UpdateAS4MessageAsync(null, null, CancellationToken.None).IsCompleted);
        }

        [Fact]
        public void SaveComplete()
        {
            Assert.True(StubMessageBodyStore.Default.SaveAS4MessageAsync(null, null, CancellationToken.None).IsCompleted);
        }

        [Fact]
        public async Task LoadsEmpty()
        {
            Assert.Equal(Stream.Null, await StubMessageBodyStore.Default.LoadMessagesBody(null));
        }

        [Fact]
        public async Task GetsConfiguredLocation()
        {
            // Arrange
            string expected = Guid.NewGuid().ToString();

            // Act
            string actual = await new StubMessageBodyStore(expected).GetMessageLocation(null, null);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
