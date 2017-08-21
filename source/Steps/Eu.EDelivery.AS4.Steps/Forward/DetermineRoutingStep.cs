using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps.Forward
{
    public class DetermineRoutingStep : IStep
    {
        private readonly IConfig _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DetermineRoutingStep"/> class.
        /// </summary>
        public DetermineRoutingStep() : this(Config.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DetermineRoutingStep"/> class.
        /// </summary>
        public DetermineRoutingStep(IConfig configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext"/>.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            ValidateMessagingContext(messagingContext);

            string sendingPModeId = messagingContext.ReceivingPMode.MessageHandling.ForwardInformation.SendingPMode;

            if (String.IsNullOrWhiteSpace(sendingPModeId))
            {
                throw new ConfigurationErrorsException("The Receiving PMode does not contain a SendingPMode-Id in the MessageHandling.Forward element");
            }

            var sendingPMode = _configuration.GetSendingPMode(sendingPModeId);

            if (sendingPMode == null)
            {
                throw new ConfigurationErrorsException($"No Sending Processing Mode found for {sendingPModeId}");
            }

            messagingContext.SendingPMode = sendingPMode;

            return StepResult.SuccessAsync(messagingContext);
        }

        private static void ValidateMessagingContext(MessagingContext messagingContext)
        {
            if (messagingContext.ReceivedMessage == null)
            {
                throw new InvalidOperationException("DetermineRoutingStep requires a MessagingContext with a ReceivedMessage");
            }

            if (messagingContext.ReceivingPMode == null)
            {
                throw new InvalidOperationException("No Receiving PMode available in MessagingContext");
            }

            if (messagingContext.ReceivingPMode.MessageHandling?.ForwardInformation == null)
            {
                throw new ConfigurationErrorsException("The Receiving PMode does not contain a MessageHandling.Forward element");
            }
        }
    }
}
