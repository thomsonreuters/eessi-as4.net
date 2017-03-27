using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;

namespace Eu.EDelivery.AS4.UnitTests.Steps
{
    /// <summary>
    /// <see cref="IStep"/> implementation to "spy" on the step execution.
    /// </summary>
    public class SpyStep : IStep
    {
        /// <summary>
        /// Gets a value indicating whether the step is executed.
        /// </summary>
        public bool IsCalled { get; private set; }

        /// <summary>
        /// Execute the step for a given <paramref name="internalMessage"/>.
        /// </summary>
        /// <param name="internalMessage">Message used during the step execution.</param>
        /// <param name="cancellationToken">Cancellation during the step execution.</param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            IsCalled = true;
            return StepResult.SuccessAsync(internalMessage);
        }
    }
}
