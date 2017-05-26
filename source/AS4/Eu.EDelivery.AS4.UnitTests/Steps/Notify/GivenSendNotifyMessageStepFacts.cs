using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Notify;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Strategies.Sender;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Notify
{
    /// <summary>
    /// Testing <see cref="SendNotifyMessageStep" />
    /// </summary>
    public class GivenSendNotifyMessageStepFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task ThenExecuteStepFailsWithConnectionFailureAsync()
        {
            // Arrange
            IStep sut = CreateSendNotifyStepWithSender(new SaboteurSender());

            var notifyMessage = new NotifyMessageEnvelope(new MessageInfo(), Status.Delivered, null, string.Empty);
            var internalMessage = new MessagingContext(notifyMessage);

            // Act / Assert
            AS4Exception exception =
                await Assert.ThrowsAsync<AS4Exception>(
                    () => sut.ExecuteAsync(internalMessage, CancellationToken.None));

            Assert.Equal(ErrorAlias.ConnectionFailure, exception.ErrorAlias);
        }

        [Fact]
        public async Task ThenExecuteStepSucceedsWithSendingPModeAsync()
        {
            // Arrange
            NotifyMessageEnvelope notifyMessage = EmptyNotifyMessageEnvelope(Status.Delivered);
            var internalMessage = new MessagingContext(notifyMessage)
            {
                SendingPMode = CreateDefaultSendingPMode()
            };

            var spySender = new SpySender();
            IStep sut = CreateSendNotifyStepWithSender(spySender);

            // Act
            await sut.ExecuteAsync(internalMessage, CancellationToken.None);

            // Assert
            Assert.True(spySender.IsNotified);
        }

        private static SendingProcessingMode CreateDefaultSendingPMode()
        {
            return new SendingProcessingMode {ReceiptHandling = {NotifyMethod = new Method()}};
        }

        [Fact]
        public async Task ThenExecuteStepWithReceivingPModeAsync()
        {
            // Arrange
            NotifyMessageEnvelope notifyMessage = EmptyNotifyMessageEnvelope(Status.Error);

            var internalMessage = new MessagingContext(notifyMessage)
            {
                SendingPMode = new SendingProcessingMode {ErrorHandling = {NotifyMethod = new Method()}}
            };

            var spySender = new SpySender();
            IStep sut = CreateSendNotifyStepWithSender(spySender);

            // Act
            await sut.ExecuteAsync(internalMessage, CancellationToken.None);

            // Assert
            Assert.True(spySender.IsNotified);
        }

        private IStep CreateSendNotifyStepWithSender(INotifySender sender)
        {
            var stubProvider = new Mock<INotifySenderProvider>();
            stubProvider.Setup(p => p.GetNotifySender(It.IsAny<string>())).Returns(sender);

            return new SendNotifyMessageStep(stubProvider.Object, GetDataStoreContext);
        }

        private static NotifyMessageEnvelope EmptyNotifyMessageEnvelope(Status status)
        {
            return new NotifyMessageEnvelope(new MessageInfo(), status, null, string.Empty);
        }
    }
}