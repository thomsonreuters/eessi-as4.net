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
        private readonly Func<InternalMessage, bool> _condition;
        private readonly Model.Internal.Steps _thenStepConfig;
        private readonly Model.Internal.Steps _elseStepConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalStep"/>
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="thenStepConfig"></param>
        /// <param name="elseStepConfig"></param>
        public ConditionalStep(Func<InternalMessage, bool> condition, Model.Internal.Steps thenStepConfig, Model.Internal.Steps elseStepConfig)
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
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            if (this._condition(internalMessage))
            {
                var steps = StepBuilder.FromSettings(this._thenStepConfig).Build();
                return await steps.ExecuteAsync(internalMessage, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var steps = StepBuilder.FromSettings(this._elseStepConfig).Build();
                return await steps.ExecuteAsync(internalMessage, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
