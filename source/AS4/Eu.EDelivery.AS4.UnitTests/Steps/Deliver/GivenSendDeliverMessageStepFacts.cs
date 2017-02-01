using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps.Deliver;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.Xml;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Deliver
{
    /// <summary>
    /// Testing <see cref="SendDeliverMessageStep"/>
    /// </summary>
    public class GivenSendDeliverMessageStepFacts
    {
        private SendDeliverMessageStep _step;
        private readonly Mock<IDeliverSender> _mockedSender;
        private Mock<IDeliverSenderProvider> _mockedProvider;

        public GivenSendDeliverMessageStepFacts()
        {
            this._mockedSender = new Mock<IDeliverSender>();
            SetupMockedProvider();
            this._step = new SendDeliverMessageStep(this._mockedProvider.Object);
        }

        private void SetupMockedProvider()
        {
            this._mockedProvider = new Mock<IDeliverSenderProvider>();
            this._mockedProvider
                .Setup(p => p.GetDeliverSender(It.IsAny<string>()))
                .Returns(this._mockedSender.Object);
        }

        public class GivenValidArguments : GivenSendDeliverMessageStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepSucceedsWithValidSenderAsync()
            {
                // Arrange
                var deliverMessage = new DeliverMessage();
                var internalMessage = new InternalMessage(deliverMessage);
                internalMessage.AS4Message.ReceivingPMode = CreateDefaultReceivingPMode();

                // Act
                await base._step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                base._mockedSender.Verify(s
                    => s.Send(It.IsAny<DeliverMessage>()), Times.Once);
            }

            private static ReceivingProcessingMode CreateDefaultReceivingPMode()
            {
                return new ReceivingProcessingMode
                {
                    Deliver = { DeliverMethod = new Method() }
                };
            }
        }

        public class GivenInvalidArguments : GivenSendDeliverMessageStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepFailsWithFailedSenderAsync()
            {
                // Arrange
                SetupFailedDeliverSender();
                var deliverMessage = new DeliverMessage();
                var internalMessage = new InternalMessage(deliverMessage);
                // Act
                AS4Exception exception = await Assert.ThrowsAsync<AS4Exception>(()
                    => base._step.ExecuteAsync(internalMessage, CancellationToken.None));
                Assert.Equal(ExceptionType.ConnectionFailure, exception.ExceptionType);
            }

            private void SetupFailedDeliverSender()
            {
                base._mockedSender
                    .Setup(s => s.Send(It.IsAny<DeliverMessage>()))
                    .Throws(new AS4Exception("Failed to send Deliver Message"));
                base._step = new SendDeliverMessageStep(base._mockedProvider.Object);
            }
        }
    }
}