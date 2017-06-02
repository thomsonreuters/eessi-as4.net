using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Transformers;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    /// <summary>
    /// Testing <see cref="ExceptionToNotifyMessageTransformer"/>
    /// </summary>
    public class GivenExceptionToNotifyMessageTransformerFacts
    {
        [Fact]
        public async void ThenInExceptionIsTransformedToNotifyEnvelope()
        {
            // Arrange
            ReceivedEntityMessage receivedMessage = CreateReceivedExceptionMessage<InException>();
            var transformer = new ExceptionToNotifyMessageTransformer();

            // Act
            InternalMessage result = await transformer.TransformAsync(receivedMessage, CancellationToken.None);

            // Assert
            Assert.NotNull(result.NotifyMessage);
            Assert.Equal(
                ((ExceptionEntity) receivedMessage.Entity).EbmsRefToMessageId,
                result.NotifyMessage.MessageInfo.RefToMessageId);
        }

        [Fact]
        public async void ThenOutExceptionIsTransformedToNotifyEnvelope()
        {
            // Arrange
            ReceivedEntityMessage receivedMessage = CreateReceivedExceptionMessage<OutException>();
            var transformer = new ExceptionToNotifyMessageTransformer();

            // Act
            InternalMessage result = await transformer.TransformAsync(receivedMessage, CancellationToken.None);

            // Assert
            Assert.NotNull(result.NotifyMessage);
            Assert.Equal(
                ((ExceptionEntity) receivedMessage.Entity).EbmsRefToMessageId,
                result.NotifyMessage.MessageInfo.RefToMessageId);
        }

        [Fact]
        public async void ThenTransformSucceedsWithValidInExceptionForErrorPropertiesAsync()
        {
            // Arrange            
            ReceivedEntityMessage receivedMessage = CreateReceivedExceptionMessage<InException>();
            var transformer = new ExceptionToNotifyMessageTransformer();

            // Act
            InternalMessage internalMessage = await transformer.TransformAsync(receivedMessage, CancellationToken.None);

            // Assert
            var error = (Error) internalMessage.AS4Message.PrimarySignalMessage;
            Assert.True(error.IsFormedByException);
            Assert.False(string.IsNullOrWhiteSpace(((ExceptionEntity) receivedMessage.Entity).Exception));
            Assert.Equal(((ExceptionEntity) receivedMessage.Entity).Exception, error.Exception.Message);
        }

        private static ReceivedEntityMessage CreateReceivedExceptionMessage<T>() where T : ExceptionEntity, new()
        {
            var exception = new T
            {
                Operation = Operation.ToBeNotified,
                EbmsRefToMessageId = "somemessage-id",
                Exception = "Some Exception Message"
            };

            return new ReceivedEntityMessage(exception);
        }

        [Fact]
        public async Task FaisToTransform_IfNotSupported()
        {
            // Arrange
            var sut = new ExceptionToNotifyMessageTransformer();

            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(
                () => sut.TransformAsync(new ReceivedMessage(Stream.Null), CancellationToken.None));

            await Assert.ThrowsAnyAsync<Exception>(
                () => sut.TransformAsync(new ReceivedEntityMessage(new InMessage()), CancellationToken.None));
        }
    }
}