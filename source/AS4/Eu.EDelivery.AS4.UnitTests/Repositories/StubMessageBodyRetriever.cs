using System;
using System.IO;
using Eu.EDelivery.AS4.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Repositories
{
    public class StubMessageBodyRetriever : IAS4MessageBodyRetriever
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
        /// Loads a <see cref="Stream"/> at a given stored <paramref name="location"/>.
        /// </summary>
        /// <param name="location">The location on which the <see cref="Stream"/> is stored.</param>
        /// <returns></returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public Stream LoadAS4MessageStream(string location)
        {
            return _createStream();
        }
    }

    public class StubMessageBodyRetrieverFacts
    {
        [Fact]
        public void ReturnsFixedStream()
        {
            // Arrange
            Stream expectedStream = Stream.Null;
            var sut = new StubMessageBodyRetriever(() => expectedStream);

            // Act
            Stream actualStream = sut.LoadAS4MessageStream(location: null);

            // Assert
            Assert.Equal(expectedStream, actualStream);
        }
    }
}
