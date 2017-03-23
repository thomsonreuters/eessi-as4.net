using System.IO;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

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
        /// <param name="requestStream">Received request stream.</param>
        /// <param name="contentType">Content Type of the request stream.</param>
        /// <param name="sendingPMode">Sending Processing Mode for the given Pull Message</param>
        public ReceivedPullMessage(Stream requestStream, string contentType, SendingProcessingMode sendingPMode) : base(requestStream, contentType)
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

            message.SendingPMode = _sendingPMode;
        }
    }
}