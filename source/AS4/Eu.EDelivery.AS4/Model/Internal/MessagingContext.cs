using System;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Serialization;
using ReceptionAwareness = Eu.EDelivery.AS4.Entities.ReceptionAwareness;

namespace Eu.EDelivery.AS4.Model.Internal
{
    /// <summary>
    /// Canonical Message Format inside the Steps
    /// </summary>
    public class MessagingContext : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingContext" /> class.
        /// Create an Internal Message with a given <see cref="Core.AS4Message" />
        /// </summary>
        /// <param name="as4Message">
        /// </param>
        public MessagingContext(AS4Message as4Message)
        {
            SubmitMessage = null;
            AS4Message = as4Message;
            DeliverMessage = null;
            NotifyMessage = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingContext" /> class.
        /// Create and Internal Message with a given <see cref="Submit.SubmitMessage" />
        /// </summary>
        /// <param name="submitMessage">
        /// </param>
        public MessagingContext(SubmitMessage submitMessage)
        {
            SubmitMessage = submitMessage;
            AS4Message = null;
            DeliverMessage = null;
            NotifyMessage = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingContext"/> class.
        /// </summary>
        /// <param name="deliverMessage">The deliver message.</param>
        public MessagingContext(DeliverMessageEnvelope deliverMessage)
        {
            SubmitMessage = null;
            AS4Message = null;
            DeliverMessage = deliverMessage;
            NotifyMessage = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingContext"/> class.
        /// </summary>
        /// <param name="notifyMessage">The notify message.</param>
        public MessagingContext(NotifyMessageEnvelope notifyMessage)
        {
            SubmitMessage = null;
            AS4Message = null;
            DeliverMessage = null;
            NotifyMessage = notifyMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingContext"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public MessagingContext(AS4Exception exception)
        {
            Exception = exception;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingContext" /> class.
        /// </summary>
        /// <param name="awareness">The awareness.</param>
        public MessagingContext(ReceptionAwareness awareness)
        {
            ReceptionAwareness = awareness;
        }

        public AS4Message AS4Message { get; }

        public SubmitMessage SubmitMessage { get; }

        public DeliverMessageEnvelope DeliverMessage { get; }

        public NotifyMessageEnvelope NotifyMessage { get; }

        public ReceptionAwareness ReceptionAwareness { get; }

        public AS4Exception Exception { get; set; }

        public SendingProcessingMode SendingPMode { get; set; }

        private ReceivingProcessingMode _receivingPMode;

        public ReceivingProcessingMode ReceivingPMode
        {
            get
            {
                return _receivingPMode;
            }

            set
            {
                if (_receivingPMode != value)
                {
                    _receivingPMode = value;
                    _receivingPModeString = null;
                }
            }
        }

        private string _receivingPModeString;

        /// <summary>
        /// Gets the prefix.
        /// </summary>
        /// <value>The prefix.</value>
        public string Prefix
        {
            get
            {
                string corePrefix = AS4Message?.PrimaryUserMessage?.MessageId ?? AS4Message?.PrimarySignalMessage?.MessageId;

                string extensionPrefix = SubmitMessage?.MessageInfo.MessageId
                                         ?? DeliverMessage?.MessageInfo.MessageId ?? NotifyMessage?.MessageInfo.MessageId;

                return $"[{corePrefix ?? extensionPrefix}]";
            }
        }

        /// <summary>
        /// Gets the receiving p mode string.
        /// </summary>
        /// <returns></returns>
        public string GetReceivingPModeString()
        {
            if (string.IsNullOrWhiteSpace(_receivingPModeString))
            {
                _receivingPModeString = AS4XmlSerializer.ToString(this.ReceivingPMode);
            }

            return _receivingPModeString;
        }

        /// <summary>
        /// Clones the message.
        /// </summary>
        /// <param name="as4Message">The as4 message.</param>
        /// <returns></returns>
        public MessagingContext CloneWith(AS4Message as4Message)
        {
            return CopyContextInfoTo(new MessagingContext(as4Message));
        }

        /// <summary>
        /// Clones the message.
        /// </summary>
        /// <param name="deliverMessage">The anonymous deliver.</param>
        /// <returns></returns>
        public MessagingContext CloneWith(DeliverMessageEnvelope deliverMessage)
        {
            return CopyContextInfoTo(new MessagingContext(deliverMessage));
        }

        /// <summary>
        /// Clones the message.
        /// </summary>
        /// <param name="notifyMessage">The notify message.</param>
        /// <returns></returns>
        public MessagingContext CloneWith(NotifyMessageEnvelope notifyMessage)
        {
            return CopyContextInfoTo(new MessagingContext(notifyMessage));
        }

        private MessagingContext CopyContextInfoTo(MessagingContext context)
        {
            context.SendingPMode = SendingPMode;
            context.ReceivingPMode = ReceivingPMode;

            return context;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            AS4Message?.CloseAttachments();
        }
    }
}