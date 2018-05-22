using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Deliver;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Eu.EDelivery.AS4.UnitTests.Strategies.Sender;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Deliver
{
    /// <summary>
    /// Testing <see cref="SendDeliverMessageStep" />
    /// </summary>
    public class GivenSendDeliverMessageStepFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task ThenExecuteStepFailsWithFailedSenderAsync()
        {
            // Arrange
            DeliverMessageEnvelope envelope = EmptyDeliverMessageEnvelope();
            IStep sut = CreateSendDeliverStepWithSender(new SaboteurSender());

            // Act
            await Assert.ThrowsAnyAsync<Exception>(() => sut.ExecuteAsync(new MessagingContext(envelope)));
        }

        [Fact]
        public async Task ThenExecuteStepSucceedsWithValidSenderAsync()
        {
            // Arrange
            DeliverMessageEnvelope envelope = EmptyDeliverMessageEnvelope();

            var spySender = new Mock<IDeliverSender>();
            spySender.Setup(s => s.SendAsync(envelope))
                     .ReturnsAsync(DeliverResult.Success);

            IStep sut = CreateSendDeliverStepWithSender(spySender.Object);

            // Act
            await sut.ExecuteAsync(new MessagingContext(envelope) { ReceivingPMode = CreateDefaultReceivingPMode() });

            // Assert
            spySender.Verify(s => s.SendAsync(It.IsAny<DeliverMessageEnvelope>()), Times.Once);
        }

        private IStep CreateSendDeliverStepWithSender(IDeliverSender spySender)
        {
            var stubProvider = new Mock<IDeliverSenderProvider>();
            stubProvider.Setup(p => p.GetDeliverSender(It.IsAny<string>())).Returns(spySender);

            return new SendDeliverMessageStep(stubProvider.Object, GetDataStoreContext);
        }

        private static DeliverMessageEnvelope EmptyDeliverMessageEnvelope()
        {
            return new DeliverMessageEnvelope(
                messageInfo: new MessageInfo(),
                deliverMessage: new byte[] { },
                contentType: string.Empty);
        }

        [Fact]
        public async Task ThenExecuteMethodSucceedsWithValidUserMessageAsync()
        {
            // Arrange
            string id = Guid.NewGuid().ToString();
            GetDataStoreContext.InsertInMessage(
                CreateInMessage(id, InStatus.Received, Operation.ToBeDelivered));

            DeliverMessageEnvelope envelope = AnonymousDeliverEnvelope(id);
            IStep sut = CreateSendDeliverStepWithSender(new SpySender());

            // Act
            await sut.ExecuteAsync(new MessagingContext(envelope) {ReceivingPMode = CreateDefaultReceivingPMode()});

            // Assert
            GetDataStoreContext.AssertInMessage(id, inmessage =>
            {
                Assert.NotNull(inmessage);
                Assert.Equal(InStatus.Delivered, InStatusUtils.Parse(inmessage.Status));
                Assert.Equal(Operation.Delivered, OperationUtils.Parse(inmessage.Operation));
            });
        }

        [Theory]
        [ClassData(typeof(DeliverRetryData))]
        public async Task Reset_InMessage_Operation_ToBeDelivered_When_CurrentRetry_LessThen_MaxRetry(DeliverRetry input)
        {
            // Arrange
            string id = Guid.NewGuid().ToString();

            InMessage msg = CreateInMessage(id, InStatus.Received, Operation.Delivering);
            msg.CurrentRetryCount = input.CurrentRetryCount;
            msg.MaxRetryCount = input.MaxRetryCount;
            GetDataStoreContext.InsertInMessage(msg);

            DeliverMessageEnvelope envelope = AnonymousDeliverEnvelope(id);

            var stub = new Mock<IDeliverSender>();
            stub.Setup(s => s.SendAsync(envelope))
                .ReturnsAsync(input.DeliverResult);

            IStep sut = CreateSendDeliverStepWithSender(stub.Object);

            // Act
            await sut.ExecuteAsync(new MessagingContext(envelope) { ReceivingPMode = CreateDefaultReceivingPMode() });

            // Assert
            GetDataStoreContext.AssertInMessage(id, inMessage =>
            {
                Assert.NotNull(inMessage);

                Assert.Equal(input.ExpectedCurrentRetryCount, inMessage.CurrentRetryCount);
                Assert.Equal(input.ExpectedOperation, OperationUtils.Parse(inMessage.Operation));
                Assert.Equal(input.ExpectedStatus, InStatusUtils.Parse(inMessage.Status));
            });
        }

        private static InMessage CreateInMessage(string id, InStatus status, Operation operation)
        {
            var m = new InMessage(id);
            m.SetStatus(status);
            m.SetOperation(operation);

            return m;
        }

        private static DeliverMessageEnvelope AnonymousDeliverEnvelope(string id)
        {
            return new DeliverMessageEnvelope(
                messageInfo: new MessageInfo { MessageId = id },
                deliverMessage: new byte[] { },
                contentType: string.Empty);
        }

        private static ReceivingProcessingMode CreateDefaultReceivingPMode()
        {
            return new ReceivingProcessingMode
            {
                MessageHandling = {DeliverInformation = {DeliverMethod = new Method()}}
            };
        }
    }
}