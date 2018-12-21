using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how the state and configuration on the retry mechanism of reception awareness is stored
    /// </summary>
    [Info("Set reception awareness for the message")]
    [Description(
        "This step makes sure that reception awareness is enabled for the message that is to be sent, " +
        "if reception awareness is enabled in the sending PMode.")]
    [Obsolete(
        "Retry information for reception awareness will be stored during the execution of "
        + nameof(SetMessageToBeSentStep)
        + ", you can safely remove this step from your settings if that step is present in your Send <NormalPipeline/> (is the case by default)")]
    public class SetReceptionAwarenessStep : IStep
    {
        /// <summary>
        /// Execute the step on a given <paramref name="messagingContext"/>.
        /// </summary>
        /// <param name="messagingContext"><see cref="MessagingContext"/> on which the step must be executed.</param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            return StepResult.SuccessAsync(messagingContext);
        }
    }
}
