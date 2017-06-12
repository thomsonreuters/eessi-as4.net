using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps
{
    /// <summary>
    /// Testing <see cref="StepResult" />
    /// </summary>
    public class GivenStepResultFacts
    {
        public class CanProceed
        {
            [Fact]
            public void IsFalseIfStopExecutionIsCalled()
            {
                // Arrange
                AS4Exception exception = AS4ExceptionBuilder.WithDescription("ignored message").Build();

                // Act
                StepResult actualStepResult = StepResult.Failed(exception).AndStopExecution();

                // Assert
                Assert.False(actualStepResult.CanProceed);
            }

            [Fact]
            public void IsTrueIfStopExecutiongIsntCalled()
            {
                // Act
                StepResult actualStepResult = StepResult.Success(AnonymousMessage());

                // Assert
                Assert.True(actualStepResult.CanProceed);
            }

            private static MessagingContext AnonymousMessage()
            {
                return new MessagingContext(AS4Message.Empty, MessagingContextMode.Unknown);
            }
        }
    }
}