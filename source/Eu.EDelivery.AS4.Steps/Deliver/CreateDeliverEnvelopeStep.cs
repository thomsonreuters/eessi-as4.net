using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps.Deliver
{
    /// <summary>
    /// <see cref="IStep" /> implementation to create a <see cref="DeliverMessage" />.
    /// </summary>
    [Info("Create deliver message")]
    [Description("Step that creates a deliver message")]
    [Obsolete("DeliverMessage is already created in transformer, you can safely remove this step from your settings")]
    [NotConfigurable]
    public class CreateDeliverEnvelopeStep : IStep
    {
        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext" />.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            return StepResult.SuccessAsync(messagingContext);
        }
    }
}