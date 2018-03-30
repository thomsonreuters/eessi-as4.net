using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Transformers;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    /// <summary>
    /// Testing <see cref="ReceptionAwarenessTransformer" />
    /// </summary>
    public class GivenReceptionAwarenessTransformerFacts
    {
        public class GivenValidArguments : GivenReceptionAwarenessTransformerFacts
        {
            [Fact]
            public async Task ThenTransformSucceedsWithValidReceptionAwarenessAsync()
            {
                // Arrange
                var awareness = new ReceptionAwareness(4, "message-id");
                var receivedMessage = new ReceivedEntityMessage(awareness);
                var transformer = new ReceptionAwarenessTransformer();

                // Act
                MessagingContext messagingContext = await transformer.TransformAsync(receivedMessage);

                // Assert
                Assert.Equal(awareness, messagingContext.ReceptionAwareness);
            }
        }

        public class GivenInvalidArguments : GivenReceptionAwarenessTransformerFacts
        {
            [Fact]
            public async Task ThenTransformFailsWithoutReceivedEntityMessageAsync()
            {
                // Arrange
                var receivedMessage = new ReceivedMessage(Stream.Null, string.Empty);
                var transformer = new ReceptionAwarenessTransformer();

                // Act / Assert
                await Assert.ThrowsAnyAsync<Exception>(
                    () => transformer.TransformAsync(receivedMessage));
            }

            [Fact]
            public async Task ThenTransformFailsWithoutReceptionAwarenessAsync()
            {
                // Arrange
                var entity = new InMessage(Guid.NewGuid().ToString());
                var receivedMessage = new ReceivedEntityMessage(entity);
                var transformer = new ReceptionAwarenessTransformer();

                // Act / Assert
                await Assert.ThrowsAnyAsync<Exception>(
                    () => transformer.TransformAsync(receivedMessage));
            }
        }
    }
}