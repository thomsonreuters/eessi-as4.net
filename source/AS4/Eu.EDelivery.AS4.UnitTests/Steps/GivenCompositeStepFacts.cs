using System;
using System.Threading;
using Eu.EDelivery.AS4.Builders;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps
{
    /// <summary>
    /// Testing <seealso cref="CompositeStep" />
    /// </summary>
    public class GivenCompositeStepFacts
    {
        private readonly CompositeStep _step;
        private Mock<IStep> _mockModule;

        public GivenCompositeStepFacts()
        {
            SetupMockModule();

            this._step = new CompositeStep(
                this._mockModule.Object);
        }

        private void SetupMockModule()
        {
            this._mockModule = new Mock<IStep>();
            this._mockModule
                .Setup(
                    m => m.ExecuteAsync(
                        It.IsAny<InternalMessage>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<StepResult>().Object);
        }

        /// <summary>
        /// Testing if the transmitter succeeds
        /// </summary>
        public class GivenCompositeStepSucceeds
            : GivenCompositeStepFacts
        {
            [Fact]
            public async void ThenTransmitMessageSucceeds()
            {
                // Arrange
                AS4Message mockMessage = new AS4MessageBuilder().Build();
                var internalMessage = new InternalMessage(mockMessage);
                // Act
                StepResult result = await this._step.ExecuteAsync(
                    internalMessage,
                    CancellationToken.None);
                // Assert
                Assert.NotNull(result);
            }
        }

        /// <summary>
        /// Testing if the transmitter fails
        /// </summary>
        public class GivenCompositeStepFails
            : GivenCompositeStepFacts
        {
            [Fact]
            public void ThenCreatingTransmitterFails()
            {
                // Act / Assert
                Assert.Throws<ArgumentNullException>(
                    () => new CompositeStep(null));
            }
        }
    }
}