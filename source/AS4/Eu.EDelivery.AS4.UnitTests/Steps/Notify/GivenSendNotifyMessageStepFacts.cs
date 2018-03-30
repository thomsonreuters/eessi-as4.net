using System;
using System.Threading;
using System.Threading.Tasks;
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

            var fixture = new MessagingContext(
                EmptyNotifyMessageEnvelope(Status.Delivered));

            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(
                    () => sut.ExecuteAsync(fixture));
        }

        [Fact]
        public async Task ThenExecuteStepSucceedsWithSendingPModeAsync()
        {
            // Arrange
            var fixture = new MessagingContext(
                EmptyNotifyMessageEnvelope(Status.Delivered))
                {SendingPMode = new SendingProcessingMode {ReceiptHandling = {NotifyMethod = new Method()}}};

            var spySender = new SpySender();
            IStep sut = CreateSendNotifyStepWithSender(spySender);

            // Act
            await sut.ExecuteAsync(fixture);

            // Assert
            Assert.True(spySender.IsNotified);
        }

        [Fact]
        public async Task ThenExecuteStepWithReceivingPModeAsync()
        {
            // Arrange
            var fixture = new MessagingContext(
                EmptyNotifyMessageEnvelope(Status.Error))
                {SendingPMode = new SendingProcessingMode {ErrorHandling = {NotifyMethod = new Method()}}};

            var spySender = new SpySender();
            IStep sut = CreateSendNotifyStepWithSender(spySender);

            // Act
            await sut.ExecuteAsync(fixture);

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
            return new NotifyMessageEnvelope(
                messageInfo: new MessageInfo(), 
                statusCode: status, 
                notifyMessage: null, 
                contentType: string.Empty, 
                entityType: default(Type));
        }
    }
}