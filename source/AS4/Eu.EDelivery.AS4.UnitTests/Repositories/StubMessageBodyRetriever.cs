using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Repositories
{
    public class StubMessageBodyRetriever : IAS4MessageBodyStore
    {
        private readonly Func<Stream> _createStream;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="StubMessageBodyRetriever" /> class.
        /// </summary>
        /// <param name="createCreateStream">The create stream.</param>
        public StubMessageBodyRetriever(Func<Stream> createCreateStream)
        {
            _createStream = createCreateStream;
        }

        /// <summary>
        /// Loads a <see cref="Stream" /> at a given stored <paramref name="location" />.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public Task<Stream> LoadMessagesBody(string location)
        {
            return Task.FromResult(_createStream());
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }

    public class StubMessageBodyRetrieverFacts
    {
        [Fact]
        public async Task ReturnsFixedStreamAsync()
        {
            // Arrange
            Stream expectedStream = Stream.Null;
            var sut = new StubMessageBodyRetriever(() => expectedStream);

            // Act
            Stream actualStream = await sut.LoadMessagesBody(location: null);

            // Assert
            Assert.Equal(expectedStream, actualStream);
        }
    }
}
