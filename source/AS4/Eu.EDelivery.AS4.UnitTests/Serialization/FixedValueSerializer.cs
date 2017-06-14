using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Serialization
{
    /// <summary>
    /// <see cref="ISerializer"/> implementation to 'Stub' the serialization process.
    /// </summary>
    public class FixedValueSerializer : ISerializer
    {
        private readonly AS4Message _fixedMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedValueSerializer" /> class.
        /// </summary>
        /// <param name="as4Message">The as4 message.</param>
        public FixedValueSerializer(AS4Message as4Message)
        {
            _fixedMessage = as4Message;
        }

        /// <summary>
        /// Serializes the asynchronous.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task SerializeAsync(AS4Message message, Stream stream, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Serializes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Serialize(AS4Message message, Stream stream, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Deserializes the asynchronous.
        /// </summary>
        /// <param name="inputStream">The input stream.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task<AS4Message> DeserializeAsync(Stream inputStream, string contentType, CancellationToken cancellationToken)
        {
            return Task.FromResult(_fixedMessage);
        }
    }

    public class FixedValueSerializerFacts
    {
        [Fact]
        public async Task FailsToSerialize()
        {
            // Arrange
            var sut = new FixedValueSerializer(as4Message: null);

            // Assert
            Assert.ThrowsAny<Exception>(
                () => sut.Serialize(message: null, stream: null, cancellationToken: CancellationToken.None));

            await Assert.ThrowsAnyAsync<Exception>(
                () => sut.SerializeAsync(message: null, stream: null, cancellationToken: CancellationToken.None));
        }

        [Fact]
        public async Task DeserializeFixedMessage()
        {
            // Arrange
            AS4Message expectedMessage = AS4Message.Empty;
            var sut = new FixedValueSerializer(expectedMessage);

            // Act
            AS4Message actualMessage = 
                await sut.DeserializeAsync(inputStream: null, contentType: null, cancellationToken: CancellationToken.None);

            // Assert
            Assert.Equal(expectedMessage, actualMessage);
        }
    }
}
