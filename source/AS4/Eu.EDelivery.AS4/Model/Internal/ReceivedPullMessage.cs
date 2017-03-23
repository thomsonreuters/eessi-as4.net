using System.IO;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Validators;

namespace Eu.EDelivery.AS4.Model.Internal
{
    /// <summary>
    /// Received Pull Request Message
    /// </summary>
    public class ReceivedPullMessage : ReceivedMessage
    {
        private readonly SendingProcessingMode _sendingPMode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedPullMessage" /> class.
        /// </summary>
        /// <param name="sendingPMode">Sending Processing Mode for the given Pull Message</param>
        public ReceivedPullMessage(SendingProcessingMode sendingPMode) : base(Stream.Null, Constants.ContentTypes.Soap)
        {
            _sendingPMode = sendingPMode;
        }

        /// <summary>
        /// Assign custom properties to the <see cref="ReceivedMessage" />
        /// </summary>
        /// <param name="message"></param>
        public override void AssignProperties(AS4Message message)
        {
            base.AssignProperties(message);

            IValidator<SendingProcessingMode> pmodeValidator = new SendingProcessingModeValidator();
            pmodeValidator.Validate(_sendingPMode);

            message.SendingPMode = _sendingPMode;
            message.SignalMessages.Add(new PullRequest(_sendingPMode.MessagePackaging.Mpc));
        }
    }
}