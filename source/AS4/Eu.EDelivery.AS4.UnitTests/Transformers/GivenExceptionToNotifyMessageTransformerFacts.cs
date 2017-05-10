using System.Threading;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Transformers;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    public class GivenExceptionToNotifyMessageTransformerFacts
    {
        [Fact]
        public async void ThenInExceptionIsTransformedToNotifyEnvelope()
        {
            var receivedMessage = CreateReceivedExceptionMessage<InException>();

            var transformer = new ExceptionToNotifyMessageTransformer();
            var result = await transformer.TransformAsync(receivedMessage, CancellationToken.None);

            Assert.NotNull(result.NotifyMessage);
            Assert.Equal(((ExceptionEntity)receivedMessage.Entity).EbmsRefToMessageId, result.NotifyMessage.MessageInfo.RefToMessageId);
        }

        [Fact]
        public async void ThenOutExceptionIsTransformedToNotifyEnvelope()
        {
            var receivedMessage = CreateReceivedExceptionMessage<OutException>();

            var transformer = new ExceptionToNotifyMessageTransformer();
            var result = await transformer.TransformAsync(receivedMessage, CancellationToken.None);

            Assert.NotNull(result.NotifyMessage);
            Assert.Equal(((ExceptionEntity)receivedMessage.Entity).EbmsRefToMessageId, result.NotifyMessage.MessageInfo.RefToMessageId);
        }

        [Fact]
        public async void ThenTransformSucceedsWithValidInExceptionForErrorPropertiesAsync()
        {
            // Arrange            
            var receivedMessage = CreateReceivedExceptionMessage<InException>();
            var transformer = new ExceptionToNotifyMessageTransformer();

            // Act
            InternalMessage internalMessage =
                await transformer.TransformAsync(receivedMessage, CancellationToken.None);

            // Assert
            var error = (Error)internalMessage.AS4Message.PrimarySignalMessage;
            Assert.True(error.IsFormedByException);
            Assert.False(string.IsNullOrWhiteSpace(((ExceptionEntity)receivedMessage.Entity).Exception));
            Assert.Equal(((ExceptionEntity)receivedMessage.Entity).Exception, error.Exception.Message);
        }

        private static ReceivedEntityMessage CreateReceivedExceptionMessage<T>() where T : ExceptionEntity, new()
        {
            var exception = new T()
            {
                Operation = Operation.ToBeNotified,
                EbmsRefToMessageId = "somemessage-id",
                Exception = "Some Exception Message"
            };

            var receivedMessage = new ReceivedEntityMessage(exception);

            return receivedMessage;
        }
    }
}