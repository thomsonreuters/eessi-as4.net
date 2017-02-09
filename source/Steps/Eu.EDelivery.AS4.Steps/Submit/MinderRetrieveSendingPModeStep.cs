using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// Retrieve Sending PMode based on Service/Action information
    /// </summary>
    public class MinderRetrieveSendingPModeStep : IStep
    {
        private readonly IConfig _config;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MinderRetrieveSendingPModeStep"/> class
        /// </summary>
        public MinderRetrieveSendingPModeStep()
        {
            this._config = Config.Instance;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._logger.Info("Minder Retrieve Sending PMode");
            AssignSendingPMode(internalMessage);

            return StepResult.SuccessAsync(internalMessage);
        }

        private void AssignSendingPMode(InternalMessage internalMessage)
        {
            CollaborationInfo collaborationInfo = internalMessage.AS4Message.PrimaryUserMessage.CollaborationInfo;
            string pmodeIdAction = SubStringPModeIdFrom(collaborationInfo.Action);

            internalMessage.AS4Message.SendingPMode = this._config.GetSendingPMode(pmodeIdAction);
        }

        private static string SubStringPModeIdFrom(string target)
        {
            return target.Substring(6);
        }
    }
}
