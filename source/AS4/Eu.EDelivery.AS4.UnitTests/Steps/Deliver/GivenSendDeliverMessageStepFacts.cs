using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Deliver;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.UnitTests.Strategies.Sender;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Deliver
{
    /// <summary>
    /// Testing <see cref="SendDeliverMessageStep" />
    /// </summary>
    public class GivenSendDeliverMessageStepFacts
    {
        [Fact]
        public async Task ThenExecuteStepFailsWithFailedSenderAsync()
        {
            // Arrange
            InternalMessage internalMessage = EmptyDeliverMessageEnvelope();
            IStep sut = CreateSendDeliverStepWithSender(new SaboteurSender());

            // Act
            AS4Exception exception =
                await Assert.ThrowsAsync<AS4Exception>(() => sut.ExecuteAsync(internalMessage, CancellationToken.None));

            Assert.Equal(ErrorAlias.ConnectionFailure, exception.ErrorAlias);
        }

        [Fact]
        public async Task ThenExecuteStepSucceedsWithValidSenderAsync()
        {
            // Arrange
            InternalMessage internalMessage = EmptyDeliverMessageEnvelope();
            internalMessage.AS4Message = new AS4Message { ReceivingPMode = CreateDefaultReceivingPMode() };

            var spySender = Mock.Of<IDeliverSender>();
            IStep sut = CreateSendDeliverStepWithSender(spySender);

            // Act
            await sut.ExecuteAsync(internalMessage, CancellationToken.None);

            // Assert
            Mock.Get(spySender).Verify(s => s.SendAsync(It.IsAny<DeliverMessageEnvelope>()), Times.Once);
        }

        private static IStep CreateSendDeliverStepWithSender(IDeliverSender spySender)
        {
            var stubProvider = new Mock<IDeliverSenderProvider>();
            stubProvider.Setup(p => p.GetDeliverSender(It.IsAny<string>())).Returns(spySender);

            return new SendDeliverMessageStep(stubProvider.Object);
        }

        private static InternalMessage EmptyDeliverMessageEnvelope()
        {
            var deliverMessage = new DeliverMessageEnvelope(new MessageInfo(), new byte[] { }, string.Empty);

            return new InternalMessage(deliverMessage);
        }

        private static ReceivingProcessingMode CreateDefaultReceivingPMode()
        {
            return new ReceivingProcessingMode {Deliver = {DeliverMethod = new Method()}};
        }
    }
}