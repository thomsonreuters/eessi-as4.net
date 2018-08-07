using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how the state and configuration on the retry mechanism of reception awareness is stored
    /// </summary>
    [Obsolete("The ReceptionAwareness is now implemented as a part of the RetryAgent instead of an seperated one.")]
    [Info("Set reception awareness for the message")]
    [Description(
        "This step makes sure that reception awareness is enabled for the message that is to be sent, " +
        "if reception awareness is enabled in the sending PMode.")]
    public class SetReceptionAwarenessStep : IStep
    {
        /// <summary>
        /// Start configuring Reception Awareness
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            return StepResult.SuccessAsync(messagingContext);
        }
    }
}