using System;
using System.Threading;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.UnitTests.Model;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps
{
    /// <summary>
    /// Testing <seealso cref="CompositeStep" />
    /// </summary>
    public class GivenCompositeStepFacts
    {
        /// <summary>
        /// Testing if the transmitter succeeds
        /// </summary>
        public class GivenCompositeStepSucceeds : GivenCompositeStepFacts
        {
            [Fact]
            public async void ThenTransmitMessageSucceeds()
            {
                // Arrange
                MessagingContext dummyMessage = CreateDummyMessageWithAttachment();
                StepResult expectedStepResult = StepResult.Success(dummyMessage);

                var compositeStep = new CompositeStep(CreateMockStepWith(expectedStepResult).Object);

                // Act
                StepResult actualStepResult = await compositeStep.ExecuteAsync(dummyMessage);

                // Assert
                Assert.Equal(expectedStepResult.MessagingContext, actualStepResult.MessagingContext);
            }

            [Fact]
            public async void ThenStepStopExecutionWithMarkedStepResult()
            {
                // Arrange
                MessagingContext expectedMessage = CreateDummyMessageWithAttachment();
                StepResult stopExecutionResult = StepResult.Success(expectedMessage).AndStopExecution();

                var spyStep = new SpyStep();
                var compositeStep = new CompositeStep(CreateMockStepWith(stopExecutionResult).Object, spyStep);

                // Act
                StepResult actualResult = await compositeStep.ExecuteAsync(new EmptyMessagingContext());

                // Assert  
                Assert.False(spyStep.IsCalled);
                Assert.Equal(expectedMessage, actualResult.MessagingContext);
            }

            private static MessagingContext CreateDummyMessageWithAttachment()
            {

                AS4Message message = AS4Message.Empty;
                message.AddAttachment(new Attachment());

                return new MessagingContext(message, MessagingContextMode.Unknown);
            }

            private static Mock<IStep> CreateMockStepWith(StepResult stepResult)
            {
                var mockStep = new Mock<IStep>();

                mockStep.Setup(m => m.ExecuteAsync(It.IsAny<MessagingContext>()))
                        .ReturnsAsync(stepResult);

                return mockStep;
            }
        }

        /// <summary>
        /// Testing if the transmitter fails
        /// </summary>
        public class GivenCompositeStepFails : GivenCompositeStepFacts
        {
            [Fact]
            public void ThenCreatingTransmitterFails()
            {
                // Act / Assert
                Assert.Throws<ArgumentNullException>(() => new CompositeStep(steps: null));
            }
        }
    }
}