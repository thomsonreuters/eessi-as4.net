using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.UnitTests.Strategies.Sender;

namespace Eu.EDelivery.AS4.UnitTests.Steps
{
    /// <summary>
    /// <see cref="IStep"/> implementation to sabotage the step execution.
    /// </summary>
    internal class SaboteurStep : IStep
    {
        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext"/>.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            throw new SaboteurException("Sabotage Step Execution");
        }
    }
}
