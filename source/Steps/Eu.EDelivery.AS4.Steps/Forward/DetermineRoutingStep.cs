using System;
using System.ComponentModel;
using System.Configuration;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Forward
{
    [Info("Determine routing for message that must be forwarded")]
    [Description("Determine how the message must be forwarded by retrieving the sending pmode that must be used.")]
    public class DetermineRoutingStep : IStep
    {
        private readonly IConfig _configuration;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            ValidateMessagingContext(messagingContext);

            string sendingPModeId = messagingContext.ReceivingPMode.MessageHandling.ForwardInformation.SendingPMode;
            Logger.Trace(
                $"{messagingContext.LogTag} Sending PMode {sendingPModeId} " +
                $"must be used to forward Message with Id {messagingContext.EbmsMessageId}");

            if (String.IsNullOrWhiteSpace(sendingPModeId))
            {
                throw new ConfigurationErrorsException(
                    "The Receiving PMode does not contain a SendingPMode-Id in the MessageHandling.Forward element." +
                    "This SendingPMode-Id is required in a Forwarding scenario and will be used to forward the message to the next MSH.");
            }

            SendingProcessingMode sendingPMode = _configuration.GetSendingPMode(sendingPModeId);

            if (sendingPMode == null)
            {
                throw new ConfigurationErrorsException(
                    $"No Sending Processing Mode found for {sendingPModeId}." +
                    "Please provide a valid Id that points to a configured Sending PMode." + 
                    @"Sending PModes are configured in the .\config\send-pmodes\ folder.");
            }

            messagingContext.SendingPMode = sendingPMode;
            return StepResult.SuccessAsync(messagingContext);
        }

        private static void ValidateMessagingContext(MessagingContext messagingContext)
        {
            if (messagingContext.ReceivedMessage == null)
            {
                throw new InvalidOperationException(
                    "DetermineRoutingStep requires a MessagingContext with a ReceivedMessage");
            }

            if (messagingContext.ReceivingPMode == null)
            {
                throw new InvalidOperationException(
                    "No Receiving PMode available in the MessagingContext." +
                    "The Receiving PMode is used to correctly forward the message with the provided configured values inside this PMode.");
            }

            if (messagingContext.ReceivingPMode.MessageHandling?.ForwardInformation == null)
            {
                throw new ConfigurationErrorsException(
                    "The Receiving PMode does not contain a MessageHandling.Forward element." +
                    "This element is required in a Forwarding scenario.");
            }
        }
    }
}
