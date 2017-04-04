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

            _steps = steps;
        }

        /// <summary>
        /// Send message through the Use Case
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {            
            InternalMessage messageToSend = internalMessage;

            foreach (IStep step in _steps)
            {
                StepResult result = await step.ExecuteAsync(messageToSend, cancellationToken);

                if (result.InternalMessage != null)
                {
                    messageToSend = result.InternalMessage;
                }

                if (!result.CanProceed)
                {
                    break;
                }
            }

            return await StepResult.SuccessAsync(messageToSend);
        }        
    }
}