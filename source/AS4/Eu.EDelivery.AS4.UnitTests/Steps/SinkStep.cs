using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;

namespace Eu.EDelivery.AS4.UnitTests.Steps
{
    /// <summary>
    /// <see cref="IStep"/> implementation to 'sink' the request.
    /// </summary>
    public class SinkStep : IStep
    {
        /// <summary>
        /// Execute the step for a given <paramref name="internalMessage"/>.
        /// </summary>
        /// <param name="internalMessage">Message used during the step execution.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            return await StepResult.SuccessAsync(internalMessage);
        }
    }
}
