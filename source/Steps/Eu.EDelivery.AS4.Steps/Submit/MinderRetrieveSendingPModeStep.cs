using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// Retrieve Sending PMode based on Service/Action information
    /// </summary>
    public class MinderRetrieveSendingPModeStep : IStep
    {
        private readonly IConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="MinderRetrieveSendingPModeStep"/> class
        /// </summary>
        public MinderRetrieveSendingPModeStep()
        {
            this._config = Config.Instance;
        }

        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            CollaborationInfo collaborationInfo = internalMessage.AS4Message.PrimaryUserMessage.CollaborationInfo;
            string pmodeIdAction = SubStringPModeId(collaborationInfo.Action);
            internalMessage.AS4Message.SendingPMode = this._config.GetSendingPMode(pmodeIdAction);

            return StepResult.SuccessAsync(internalMessage);
        }

        private string SubStringPModeId(string target)
        {
            return target.Substring(4);
        }
    }
}
