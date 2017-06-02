using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps.Notify;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.UnitTests.Common;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Notify
{
    /// <summary>
    /// Testing <see cref="SendNotifyMessageStep" />
    /// </summary>
    public class GivenSendNotifyMessageStepFacts : GivenDatastoreFacts
    {
        private readonly Mock<INotifySenderProvider> _mockedProvider;
        private readonly Mock<INotifySender> _mockedSender;
        private SendNotifyMessageStep _step;

        public GivenSendNotifyMessageStepFacts()
        {
            _mockedSender = new Mock<INotifySender>();
            _mockedProvider = new Mock<INotifySenderProvider>();
            _mockedProvider.Setup(p => p.GetNotifySender(It.IsAny<string>())).Returns(_mockedSender.Object);

            _step = new SendNotifyMessageStep(_mockedProvider.Object, () => new DatastoreContext(Options));
        }

        public class GivenValidArguments : GivenSendNotifyMessageStepFacts
        {
            private static SendingProcessingMode CreateDefaultSendingPMode()
            {
                return new SendingProcessingMode {ReceiptHandling = {NotifyMethod = new Method()}};
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithSendingPModeAsync()
            {
                // Arrange
                var notifyMessage = new NotifyMessageEnvelope(new MessageInfo(), Status.Delivered, null, string.Empty);
                var internalMessage = new InternalMessage(notifyMessage)
                {
                    AS4Message = {
                                    SendingPMode = CreateDefaultSendingPMode()
                                 }
                };

                // Act
                await _step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                _mockedSender.Verify(s => s.SendAsync(It.IsAny<NotifyMessageEnvelope>()), Times.AtLeastOnce);
            }

            [Fact]
            public async Task ThenExecuteStepWithReceivingPModeAsync()
            {
                // Arrange
                var notifyMessage = new NotifyMessageEnvelope(new MessageInfo(), Status.Error, null, string.Empty);

                var internalMessage = new InternalMessage(notifyMessage)
                {
                    AS4Message =
                    {
                        SendingPMode = new SendingProcessingMode {ErrorHandling = {NotifyMethod = new Method()}}
                    }
                };

                // Act
                await _step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                _mockedSender.Verify(s => s.SendAsync(It.IsAny<NotifyMessageEnvelope>()));
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
                AS4Exception exception =
                    await Assert.ThrowsAsync<AS4Exception>(
                        () => _step.ExecuteAsync(internalMessage, CancellationToken.None));
                Assert.Equal(ErrorAlias.ConnectionFailure, exception.ErrorAlias);
            }

            private void SetupFailedNotifySender()
            {
                _mockedSender.Setup(s => s.SendAsync(It.IsAny<NotifyMessageEnvelope>())).Throws<Exception>();
                _step = new SendNotifyMessageStep(_mockedProvider.Object, () => new DatastoreContext(Options));
            }
        }
    }
}