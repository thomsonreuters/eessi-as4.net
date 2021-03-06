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
        public Task<Stream> LoadMessageBodyAsync(string location)
        {
            return Task.FromResult(_createStream());
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
            throw new NotImplementedException();
        }

        public Task<string> SaveAS4MessageStreamAsync(string location, Stream as4MessageStream)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates an existing AS4 Message body.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="message">The message that should overwrite the existing messagebody.</param>
        /// <returns></returns>
        public void UpdateAS4Message(string location, AS4Message message)
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
            Stream actualStream = await sut.LoadMessageBodyAsync(location: null);

            // Assert
            Assert.Equal(expectedStream, actualStream);
        }
    }
}
