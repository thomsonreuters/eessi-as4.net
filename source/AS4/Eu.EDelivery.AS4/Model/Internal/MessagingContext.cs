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
        /// Initializes a new instance of the <see cref="MessagingContext"/> class.
        /// </summary>
        public MessagingContext(ReceivedMessage receivedMessage, MessagingContextMode mode)
        {
            SubmitMessage = null;
            ReceivedMessage = receivedMessage;
            AS4Message = null;
            DeliverMessage = null;
            NotifyMessage = null;
            Mode = mode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingContext" /> class.
        /// Create an Internal Message with a given <see cref="Core.AS4Message" />
        /// </summary>
        /// <param name="as4Message"> </param>
        /// <param name="mode">The <see cref="MessagingContextMode"/> in which the context is currently acting</param>
        public MessagingContext(AS4Message as4Message, MessagingContextMode mode)
        {
            SubmitMessage = null;
            ReceivedMessage = null;
            AS4Message = as4Message;
            DeliverMessage = null;
            NotifyMessage = null;
            Mode = mode;
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
            ReceivedMessage = null;
            AS4Message = null;
            DeliverMessage = null;
            NotifyMessage = null;
            Mode = MessagingContextMode.Submit;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingContext"/> class.
        /// </summary>
        /// <param name="deliverMessage">The deliver message.</param>
        public MessagingContext(DeliverMessageEnvelope deliverMessage)
        {
            SubmitMessage = null;
            ReceivedMessage = null;
            AS4Message = null;
            DeliverMessage = deliverMessage;
            NotifyMessage = null;
            Mode = MessagingContextMode.Deliver;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingContext"/> class.
        /// </summary>
        /// <param name="notifyMessage">The notify message.</param>
        public MessagingContext(NotifyMessageEnvelope notifyMessage)
        {
            SubmitMessage = null;
            ReceivedMessage = null;
            AS4Message = null;
            DeliverMessage = null;
            NotifyMessage = notifyMessage;
            Mode = MessagingContextMode.Notify;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingContext" /> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public MessagingContext(Exception exception)
        {
            Exception = exception;
            Mode = MessagingContextMode.Unknown;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingContext" /> class.
        /// </summary>
        /// <param name="awareness">The awareness.</param>
        public MessagingContext(ReceptionAwareness awareness)
        {
            ReceptionAwareness = awareness;
            Mode = MessagingContextMode.Unknown;
        }

        public ReceivedMessage ReceivedMessage { get; private set; }

        public AS4Message AS4Message { get; private set; }

        public SubmitMessage SubmitMessage { get; private set; }

        public DeliverMessageEnvelope DeliverMessage { get; private set; }

        public NotifyMessageEnvelope NotifyMessage { get; private set; }

        public ReceptionAwareness ReceptionAwareness { get; }

        public MessagingContextMode Mode { get; private set; }

        public Exception Exception { get; set; }

        public ErrorResult ErrorResult { get; set; }

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
        /// Gets the Id of the Message that is handled by this context.
        /// </summary>
        /// <value>The message identifier.</value>
        public string EbmsMessageId
        {
            get
            {
                if (AS4Message != null)
                {
                    return AS4Message.GetPrimaryMessageId();
                }

                if (DeliverMessage != null)
                {
                    return DeliverMessage.MessageInfo.MessageId;
                }

                if (NotifyMessage != null)
                {
                    return NotifyMessage.MessageInfo.RefToMessageId;
                }

                return string.Empty;
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
                _receivingPModeString = AS4XmlSerializer.ToString(ReceivingPMode);
            }

            return _receivingPModeString;
        }

        public bool ReceivedMessageMustBeForwarded => ReceivingPMode?.MessageHandling?.MessageHandlingType == MessageHandlingChoiceType.Forward;

        /// <summary>
        /// Modifies the MessagingContext
        /// </summary>
        /// <param name="as4Message">The as4 message.</param>
        /// <returns></returns>
        public void ModifyContext(AS4Message as4Message)
        {
            PrepareContextChange();
            AS4Message = as4Message;
        }

        public void ModifyContext(ReceivedMessage receivedMessage)
        {
            PrepareContextChange();
            ReceivedMessage = receivedMessage;
        }

        /// <summary>
        /// Modifies the MessagingContext
        /// </summary>
        /// <param name="deliverMessage">The anonymous deliver.</param>
        /// <returns></returns>
        public void ModifyContext(DeliverMessageEnvelope deliverMessage)
        {
            PrepareContextChange();
            DeliverMessage = deliverMessage;
            Mode = MessagingContextMode.Deliver;
        }

        /// <summary>
        /// Clones the message.
        /// </summary>
        /// <param name="notifyMessage">The notify message.</param>
        /// <returns></returns>
        public void ModifyContext(NotifyMessageEnvelope notifyMessage)
        {
            PrepareContextChange();
            NotifyMessage = notifyMessage;
            Mode = MessagingContextMode.Notify;
        }

        private void PrepareContextChange()
        {
            SubmitMessage = null;
            AS4Message = null;
            NotifyMessage = null;
            DeliverMessage = null;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            AS4Message?.CloseAttachments();
            ReceivedMessage?.UnderlyingStream?.Dispose();
        }
    }

    public enum MessagingContextMode
    {
        Unknown,
        Submit,
        Send,
        Receive,
        Deliver,
        Forward,
        Notify
    }
}