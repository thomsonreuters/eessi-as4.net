using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps
{
    /// <summary>
    /// <see cref="IStep"/> implementation to "spy" on the step execution.
    /// </summary>
    internal class SpyStep : IStep
    {
        /// <summary>
        /// Gets a value indicating whether the step is executed.
        /// </summary>
        public bool IsCalled { get; private set; }

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext"/>.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            IsCalled = true;
            return StepResult.SuccessAsync(messagingContext);
        }

    }

    public class SpyStepFacts
    {
        [Fact]
        public async Task TestSpyStep()
        {
            // Arrange
            var step = new SpyStep();

            // Act
            await step.ExecuteAsync(messagingContext: null);

            // Assert
            Assert.True(step.IsCalled);
        }
    }
}
