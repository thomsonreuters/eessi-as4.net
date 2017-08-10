using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    public class DetermineMessageHandlingStep : IStep
    {
        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext"/>.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            if (messagingContext.ReceivingPMode == null)
            {
                throw new InvalidOperationException("The Receiving PMode for this message has not been determined yet.");
            }

            throw new NotImplementedException();
        }
    }
}
