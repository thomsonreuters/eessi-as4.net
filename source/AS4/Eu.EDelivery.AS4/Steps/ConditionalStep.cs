using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps
{
    /// <summary>
    /// <see cref="IStep"/>
    /// </summary>
    public class ConditionalStep : IStep
    {
        private readonly Func<AS4Message, bool> _condition;
        private readonly Model.Internal.Steps _thenStepConfig;
        private readonly Model.Internal.Steps _elseStepConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalStep"/>
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="thenStepConfig"></param>
        /// <param name="elseStepConfig"></param>
        public ConditionalStep(Func<AS4Message, bool> condition, Model.Internal.Steps thenStepConfig, Model.Internal.Steps elseStepConfig)
        {
            this._condition = condition;
            this._thenStepConfig = thenStepConfig;
            this._elseStepConfig = elseStepConfig;
        }

        /// <summary>
        /// Run the selected step
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            if (this._condition(internalMessage.AS4Message))
            {
                var steps = StepBuilder.FromSettings(this._thenStepConfig).Build();
                return steps.ExecuteAsync(internalMessage, cancellationToken);
            }
            else
            {
                var steps = StepBuilder.FromSettings(this._elseStepConfig).Build();
                return steps.ExecuteAsync(internalMessage, cancellationToken);
            }
        }
    }
}
