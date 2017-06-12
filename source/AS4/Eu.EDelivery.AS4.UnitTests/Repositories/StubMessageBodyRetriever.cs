using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Repositories
{
    public class StubMessageBodyRetriever : StubMessageBodyStore
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
        public override Task<Stream> LoadMessagesBody(string location)
        {
            return Task.FromResult(_createStream());
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
