using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps.Deliver
{
    /// <summary>
    /// Describes how the data store gets updated when an incoming message is delivered
    /// </summary>
    [Info("Update message status after delivery")]
    [Description("This step makes sure that the status of the message is correctly set after the message has been delivered.")]
    [Obsolete("The functionality of this step is now embeded in the " + nameof(SendDeliverMessageStep) + " making this step obsolete")]
    public class DeliverUpdateDatastoreStep : IStep
    {
        /// <summary>
        /// Start updating the InMessages
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            return StepResult.SuccessAsync(messagingContext);
        }
    }
}
