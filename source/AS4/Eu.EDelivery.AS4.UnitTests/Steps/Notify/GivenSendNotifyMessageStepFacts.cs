using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps.Notify;
using Eu.EDelivery.AS4.Strategies.Sender;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Notify
{
    /// <summary>
    /// Testing <see cref="SendNotifyMessageStep"/>
    /// </summary>
    public class GivenSendNotifyMessageStepFacts
    {
        private SendNotifyMessageStep _step;
        private readonly Mock<INotifySender> _mockedSender;
        private readonly Mock<INotifySenderProvider> _mockedProvider;

        public GivenSendNotifyMessageStepFacts()
        {
            this._mockedSender = new Mock<INotifySender>();
            this._mockedProvider = new Mock<INotifySenderProvider>();
            this._mockedProvider.Setup(p => p.GetNotifySender(It.IsAny<string>())).Returns(this._mockedSender.Object);

            this._step = new SendNotifyMessageStep(this._mockedProvider.Object);
        }

        public class GivenValidArguments : GivenSendNotifyMessageStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepSucceedsWithSendingPModeAsync()
            {
                // Arrange
                var notifyMessage = new NotifyMessageEnvelope(new MessageInfo(), Status.Delivered, null, string.Empty);
                var internalMessage = new InternalMessage(notifyMessage);
                internalMessage.AS4Message.SendingPMode = CreateDefaultSendingPMode();

                // Act
                await base._step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                base._mockedSender.Verify(s
                    => s.Send(It.IsAny<NotifyMessageEnvelope>()), Times.AtLeastOnce);
            }

            private SendingProcessingMode CreateDefaultSendingPMode()
            {
                return new SendingProcessingMode
                {
                    ReceiptHandling = { NotifyMethod = new Method() }
                };
            }

            [Fact]
            public async Task ThenExecuteStepWithReceivingPModeAsync()
            {
                // Arrange
                var notifyMessage = new NotifyMessageEnvelope(new MessageInfo(), Status.Error, null, String.Empty);

                var internalMessage = new InternalMessage(notifyMessage);
                internalMessage.AS4Message.SendingPMode = new SendingProcessingMode { ErrorHandling = { NotifyMethod = new Method() } };
                // Act
                await base._step.ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                base._mockedSender.Verify(s => s.Send(It.IsAny<NotifyMessageEnvelope>()));
            }
           
        }

        public class GivenInvalidArguments : GivenSendNotifyMessageStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepFailsWithConnectionFailureAsync()
            {
                // Arrange
                SetupFailedNotifySender();
                var notifyMessage = new NotifyMessageEnvelope(new MessageInfo(), Status.Delivered, null, string.Empty);
                var internalMessage = new InternalMessage(notifyMessage);
                // Act / Assert
                await AssertFailedNotifySender(internalMessage);
            }

            private async Task AssertFailedNotifySender(InternalMessage internalMessage)
            {
                AS4Exception exception = await Assert.ThrowsAsync<AS4Exception>(() =>
                        base._step.ExecuteAsync(internalMessage, CancellationToken.None));
                Assert.Equal(ExceptionType.ConnectionFailure, exception.ExceptionType);
            }

            private void SetupFailedNotifySender()
            {
                base._mockedSender
                    .Setup(s => s.Send(It.IsAny<NotifyMessageEnvelope>()))
                    .Throws<Exception>();
                base._step = new SendNotifyMessageStep(this._mockedProvider.Object);
            }
        }
    }
}