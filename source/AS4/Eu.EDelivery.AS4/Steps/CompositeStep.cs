using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps
{
    /// <summary>
    /// Composition of Steps
    /// </summary>
    [NotConfigurable]
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
            if (steps == null)
            {
                throw new ArgumentNullException(nameof(steps));
            }

            _steps = steps;
        }

        /// <summary>
        /// Send message through the Use Case
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {            
            MessagingContext messageToSend = messagingContext;
            StepResult result = StepResult.Success(messageToSend);

            foreach (IStep step in _steps)
            {
                result = await step.ExecuteAsync(messageToSend, cancellationToken).ConfigureAwait(false);

                if (result.MessagingContext != null)
                {
                    messageToSend = result.MessagingContext;
                }

                if (!result.CanProceed)
                {
                    break;
                }
            }

            return result;
        }        
    }
}