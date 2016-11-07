using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps
{
    /// <summary>
    /// Composition of Steps
    /// </summary>
    public class CompositeStep : IStep
    {
        private readonly IList<IStep> _steps;

        private InternalMessage _messageToSend;
        private StepResult _response;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeStep"/> class. 
        /// Create a <see cref="CompositeStep"/> that acts as a
        /// Composition of <see cref="IStep"/> implementations
        /// </summary>
        /// <param name="steps">
        /// </param>
        public CompositeStep(params IStep[] steps)
        {
            if (steps == null) throw new ArgumentNullException(nameof(steps));
            this._steps = steps;
        }

        /// <summary>
        /// Send message through the Use Case
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._response = new StepResult();
            this._messageToSend = internalMessage;

            foreach (IStep step in this._steps)
                await ExecuteStep(step, cancellationToken);

            return StepResult.Success(this._messageToSend);
        }

        private async Task ExecuteStep(IStep step, CancellationToken cancellationToken)
        {
            StepResult result = await step.ExecuteAsync(this._messageToSend, cancellationToken);

            if (result.Result != null && this._response.Result == null)
                this._response = result;

            if (result.InternalMessage != null)
                this._messageToSend = result.InternalMessage;
        }
    }
}