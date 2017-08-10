using System;
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
            MessagingContext messagingContext = EmptyDeliverMessageEnvelope();
            IStep sut = CreateSendDeliverStepWithSender(new SaboteurSender());

            // Act
            await Assert.ThrowsAnyAsync<Exception>(() => sut.ExecuteAsync(messagingContext, CancellationToken.None));
        }

        [Fact]
        public async Task ThenExecuteStepSucceedsWithValidSenderAsync()
        {
            // Arrange
            MessagingContext messagingContext = EmptyDeliverMessageEnvelope();
            messagingContext.ReceivingPMode = CreateDefaultReceivingPMode();

            var spySender = Mock.Of<IDeliverSender>();
            IStep sut = CreateSendDeliverStepWithSender(spySender);

            // Act
            await sut.ExecuteAsync(messagingContext, CancellationToken.None);

            // Assert
            Mock.Get(spySender).Verify(s => s.SendAsync(It.IsAny<DeliverMessageEnvelope>()), Times.Once);
        }

        private static IStep CreateSendDeliverStepWithSender(IDeliverSender spySender)
        {
            var stubProvider = new Mock<IDeliverSenderProvider>();
            stubProvider.Setup(p => p.GetDeliverSender(It.IsAny<string>())).Returns(spySender);

            return new SendDeliverMessageStep(stubProvider.Object);
        }

        private static MessagingContext EmptyDeliverMessageEnvelope()
        {
            var deliverMessage = new DeliverMessageEnvelope(new MessageInfo(), new byte[] { }, string.Empty);

            return new MessagingContext(deliverMessage);
        }

        private static ReceivingProcessingMode CreateDefaultReceivingPMode()
        {
            var pmode = new ReceivingProcessingMode();
            pmode.MessageHandling.DeliverInformation.DeliverMethod = new Method();
            return pmode;            
        }
    }
}