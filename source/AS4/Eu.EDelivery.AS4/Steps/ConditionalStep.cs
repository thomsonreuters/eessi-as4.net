using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps
{
    /// <summary>
    /// <see cref="IStep" />
    /// </summary>
    public class ConditionalStep : IStep
    {
        private readonly Func<MessagingContext, bool> _condition;
        private readonly Step[] _thenSteps;
        private readonly Step[] _elseSteps;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalStep" /> class.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="thenStepConfig"></param>
        /// <param name="elseStepConfig"></param>
        public ConditionalStep(
            Func<MessagingContext, bool> condition,
            Step[] thenStepConfig,
            Step[] elseStepConfig)
        {
            _condition = condition;
            _thenSteps = thenStepConfig;
            _elseSteps = elseStepConfig;
        }

        /// <summary>
        /// Run the selected step
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(
            MessagingContext messagingContext,
            CancellationToken cancellationToken)
        {
            if (_condition(messagingContext))
            {
                IStep steps = StepBuilder.FromSettings(_thenSteps).Build();
                return await steps.ExecuteAsync(messagingContext, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                IStep steps = StepBuilder.FromSettings(_elseSteps).Build();
                return await steps.ExecuteAsync(messagingContext, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}