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
                // Act
                StepResult actualStepResult = StepResult.Failed(context: null).AndStopExecution();

                // Assert
                Assert.False(actualStepResult.CanProceed);
                Assert.False(actualStepResult.Succeeded);
            }

            [Fact]
            public void IsTrueIfStopExecutiongIsntCalled()
            {
                // Act
                StepResult actualStepResult = StepResult.Success(AnonymousMessage());

                // Assert
                Assert.True(actualStepResult.CanProceed);
                Assert.True(actualStepResult.Succeeded);
            }

            private static MessagingContext AnonymousMessage()
            {
                return new MessagingContext(AS4Message.Empty, MessagingContextMode.Unknown);
            }
        }
    }
}