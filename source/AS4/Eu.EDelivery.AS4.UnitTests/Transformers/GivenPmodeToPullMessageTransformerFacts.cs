using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Transformers;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Eu.EDelivery.AS4.UnitTests.Model.PMode;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    /// <summary>
    /// Testing <see cref="PModeToPullMessageTransformer" />
    /// </summary>
    public class GivenPModeToPullMessageTransformerFacts
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GivenPModeToPullMessageTransformerFacts"/> class.
        /// </summary>
        public GivenPModeToPullMessageTransformerFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        [Theory]
        [ClassData(typeof(ReceivedPullMessageSource))]
        public async Task FailsWithNoPullConfigurationSection(ReceivedMessage receivedMessage)
        {
            // Arrange
            var transformer = new PModeToPullMessageTransformer();

            // Act
            InternalMessage message = await transformer.TransformAsync(receivedMessage, CancellationToken.None);

            // Assert
            Assert.NotNull(message.Exception);
            Assert.NotEmpty(message.Exception.Message);
        }

        [Fact]
        public async Task SucceedsWithAValidPullConfiguration()
        {
            // Arrange
            const string expectedMpc = "expected-mpc";
            var transformer = new PModeToPullMessageTransformer();
            var expectedSendingPMode = new ValidStubSendingPMode("expected-id") {MessagePackaging = {Mpc = expectedMpc}};
            var receivedMessage = new ReceivedPullMessage(new AS4Message().ToStream(), Constants.ContentTypes.Soap, expectedSendingPMode);

            // Act
            InternalMessage message = await transformer.TransformAsync(receivedMessage, CancellationToken.None);

            // Assert
            var actualSignalMessage = message.AS4Message.PrimarySignalMessage as PullRequest;
            Assert.Equal(expectedMpc, actualSignalMessage?.Mpc);
            Assert.Equal(expectedSendingPMode, message.AS4Message.SendingPMode);
        }

        /// <summary>
        /// Source of different <see cref="ReceivedPullMessage"/> instances.
        /// </summary>
        private class ReceivedPullMessageSource : IEnumerable<object[]>
        {
            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {new ReceivedMessage(requestStream: null)};
                yield return new object[] {new ReceivedMessage(Stream.Null)};

                var invalidSendingPMode = new ValidStubSendingPMode("my id") {PullConfiguration = new PullConfiguration()};
                MemoryStream messageStream = new AS4Message {SendingPMode = invalidSendingPMode}.ToStream();

                yield return new object[] {new ReceivedPullMessage(messageStream, Constants.ContentTypes.Soap, invalidSendingPMode) };
            }
        }
    }
}