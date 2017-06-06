using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;

namespace Eu.EDelivery.AS4.UnitTests.Steps
{
    /// <summary>
    /// <see cref="IStep"/> implementation to sabotage the step execution.
    /// </summary>
    public class SaboteurStep : IStep
    {
        private readonly Exception _fixedException;

        /// <summary>
        /// Initializes a new instance of the <see cref="SaboteurStep"/> class.
        /// </summary>
        /// <param name="exception">Confiugred exception being thrown during the execution of the <see cref="IStep"/>.</param>
        public SaboteurStep(Exception exception)
        {
            _fixedException = exception;
        }

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext"/>.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            throw _fixedException;
        }
    }
}
