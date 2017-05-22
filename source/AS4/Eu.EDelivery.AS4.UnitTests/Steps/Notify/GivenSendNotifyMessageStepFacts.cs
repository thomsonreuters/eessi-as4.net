using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps.Notify;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.UnitTests.Common;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Eu.EDelivery.AS4.Steps;

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

            _step = new SendNotifyMessageStep(_mockedProvider.Object, GetDataStoreContext);
        }

        public class GivenValidArguments : GivenSendNotifyMessageStepFacts
        {
            [Fact]
            public async Task SendPModeGetsAssignedFromDatastore_IfNotPresent()
            {
                // Arrange
                const string expectedId = "message-id";
                var expectedPMode = new SendingProcessingMode {Id = "pmode-id"};
                InsertOutMessageWithPMode(expectedId, expectedPMode);

                AS4Message receiptMessage = CreateReceiptThatReference(expectedId);
                receiptMessage.SendingPMode = null;

                var message = new InternalMessage(receiptMessage) {NotifyMessage = CreateDeliveredNotifyMessage()};

                // Act
                StepResult result = await _step.ExecuteAsync(message, CancellationToken.None);

                // Assert
                SendingProcessingMode actualPMode = result.InternalMessage.AS4Message.SendingPMode;
                Assert.Equal(expectedPMode.Id, actualPMode.Id);
            }

            private static AS4Message CreateReceiptThatReference(string expectedId)
            {
                return new AS4MessageBuilder().WithSignalMessage(new Receipt {RefToMessageId = expectedId}).Build();
            }

            private void InsertOutMessageWithPMode(string expectedId, SendingProcessingMode pmode)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    context.OutMessages.Add(
                        new OutMessage {EbmsMessageId = expectedId, PMode = AS4XmlSerializer.ToString(pmode)});
                    context.SaveChanges();
                }
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithSendingPModeAsync()
            {
                // Arrange
                NotifyMessageEnvelope notifyMessage = CreateDeliveredNotifyMessage();
                var internalMessage = new InternalMessage(notifyMessage)
                {
                    AS4Message =
                    {
                        SendingPMode = new SendingProcessingMode {ReceiptHandling = {NotifyMethod = new Method()}}
                    }
                };

                // Act
                await _step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                _mockedSender.Verify(s => s.SendAsync(It.IsAny<NotifyMessageEnvelope>()), Times.AtLeastOnce);
            }

            private static NotifyMessageEnvelope CreateDeliveredNotifyMessage()
            {
                return new NotifyMessageEnvelope(new MessageInfo(), Status.Delivered, null, string.Empty);
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
                _step = new SendNotifyMessageStep(_mockedProvider.Object, GetDataStoreContext);
            }
        }
    }
}