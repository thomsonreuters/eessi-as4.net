using System;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Serialization;
using NLog;

namespace Eu.EDelivery.AS4.Model.Internal
{
    /// <summary>
    /// Canonical Message Format inside the Steps
    /// </summary>
    public class MessagingContext : IDisposable
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingContext"/> class.
        /// </summary>
        public MessagingContext(ReceivedMessage receivedMessage, MessagingContextMode mode)
        {
            if (receivedMessage == null)
            {
                throw new ArgumentNullException(nameof(receivedMessage));
            }

            SubmitMessage = null;
            ReceivedMessage = receivedMessage;
            AS4Message = null;
            DeliverMessage = null;
            NotifyMessage = null;
            Mode = mode;

            if (ReceivedMessage is ReceivedEntityMessage receivedEntityMessage)
            {
                MessageEntityId = receivedEntityMessage.Entity?.Id;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingContext"/> class.
        /// </summary>
        /// <param name="as4Message"></param>
        /// <param name="receivedMessage"></param>
        /// <param name="mode"></param>
        public MessagingContext(
            AS4Message as4Message,
            ReceivedMessage receivedMessage,
            MessagingContextMode mode)
        {
            if (as4Message == null)
            {
                throw new ArgumentNullException(nameof(as4Message));
            }

            if (receivedMessage == null)
            {
                throw new ArgumentNullException(nameof(receivedMessage));
            }

            AS4Message = as4Message;
            ReceivedMessage = receivedMessage;
            Mode = mode;

            SubmitMessage = null;
            DeliverMessage = null;
            NotifyMessage = null;

            if (ReceivedMessage is ReceivedEntityMessage receivedEntityMessage)
            {
                MessageEntityId = receivedEntityMessage.Entity?.Id;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingContext" /> class.
        /// </summary>
        /// <param name="as4Message"> </param>
        /// <param name="mode">The <see cref="MessagingContextMode"/> in which the context is currently acting</param>
        public MessagingContext(AS4Message as4Message, MessagingContextMode mode)
        {
            if (as4Message == null)
            {
                throw new ArgumentNullException(nameof(as4Message));
            }

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
            if (submitMessage == null)
            {
                throw new ArgumentNullException(nameof(submitMessage));
            }

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
            if (deliverMessage == null)
            {
                throw new ArgumentNullException(nameof(deliverMessage));
            }

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
        /// <param name="entityId"></param>
        public MessagingContext(NotifyMessageEnvelope notifyMessage, long? entityId = null)
        {
            if (notifyMessage == null)
            {
                throw new ArgumentNullException(nameof(notifyMessage));
            }

            SubmitMessage = null;
            ReceivedMessage = null;
            AS4Message = null;
            DeliverMessage = null;
            NotifyMessage = notifyMessage;
            MessageEntityId = entityId;
            Mode = MessagingContextMode.Notify;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingContext" /> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public MessagingContext(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            Exception = exception;
            Mode = MessagingContextMode.Unknown;
        }

        public long? MessageEntityId { get; }

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

                if (SubmitMessage != null)
                {
                    return SubmitMessage.MessageInfo?.MessageId;
                }

                if (DeliverMessage != null)
                {
                    return DeliverMessage.MessageInfo?.MessageId;
                }

                if (NotifyMessage != null)
                {
                    return NotifyMessage.MessageInfo?.RefToMessageId;
                }

                if (ReceivedMessage is ReceivedEntityMessage e)
                {
                    if (e.Entity is MessageEntity me)
                    {
                        return me.EbmsMessageId;
                    }
                    if (e.Entity is ExceptionEntity ex)
                    {
                        return ex.EbmsRefToMessageId;
                    }
                }

                return string.Empty;
            }
        }

        public ReceivedMessage ReceivedMessage { get; private set; }

        public AS4Message AS4Message { get; private set; }

        public SubmitMessage SubmitMessage { get; private set; }

        public DeliverMessageEnvelope DeliverMessage { get; private set; }

        public NotifyMessageEnvelope NotifyMessage { get; private set; }

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

        /// <summary>
        /// Retrieve referenced <see cref="SendingProcessingMode"/> from a given <see cref="ReceivingProcessingMode"/>.
        /// </summary>
        /// <param name="receivePMode">The <see cref="ReceivingProcessingMode"/> that references the <see cref="SendingProcessingMode"/></param>
        /// <param name="config">The configuration used to retrieve the referenced <see cref="SendingProcessingMode"/></param>
        /// <returns></returns>
        public SendingProcessingMode GetReferencedSendingPMode(
            ReceivingProcessingMode receivePMode,
            IConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (receivePMode == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(receivePMode.ReplyHandling?.SendingPMode))
            {
                Logger.Error(
                    $"No referenced SendingPMode defined in ReplyHandling of ReceivedPMode {receivePMode.Id}. " + 
                    "This means that this PMode cannot be used to send/forward a message");

                return null;
            }

            string pmodeId = receivePMode.ReplyHandling.SendingPMode;

            try
            {
                SendingProcessingMode sendPMode = config.GetSendingPMode(pmodeId);
                if (sendPMode == null)
                {
                    Logger.Error(
                        $"Referenced SendingPMode \"{pmodeId}\"  found in ReceivingPMode{{ \"{receivePMode.Id}\"}}.ReplyHandling.SendingPMode " +
                        "does not reference an exsisting SendingPMode. Please define an existing SendingPMode id or define one at: '.\\config\\send-pmodes\\'");

                    return null;
                }

                Logger.Debug(
                    $"Found referenced SendingPMode \"{pmodeId}\", this PMode will be used to further send/forward the message");

                return sendPMode;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

        public bool ReceivedMessageMustBeForwarded => ReceivingPMode?.MessageHandling?.MessageHandlingType == MessageHandlingChoiceType.Forward;

        public string LogTag => $"({Mode})" + (string.IsNullOrEmpty(EbmsMessageId) ? "" : $"[{EbmsMessageId}]");

        /// <summary>
        /// Modifies the MessagingContext
        /// </summary>
        /// <param name="as4Message">The as4 message.</param>
        public void ModifyContext(AS4Message as4Message)
        {
            if (as4Message == null)
            {
                throw new ArgumentNullException(nameof(as4Message));
            }

            PrepareContextChange();
            AS4Message = as4Message;
        }

        /// <summary>
        /// Modifies the <see cref="MessagingContext"/>.
        /// </summary>
        /// <param name="as4Message">The as4 message.</param>
        /// <param name="mode">The mode.</param>
        public void ModifyContext(AS4Message as4Message, MessagingContextMode mode)
        {
            if (as4Message == null)
            {
                throw new ArgumentNullException(nameof(as4Message));
            }

            PrepareContextChange();
            AS4Message = as4Message;
            Mode = mode;
        }

        public void ModifyContext(ReceivedMessage receivedMessage, MessagingContextMode mode)
        {
            if (receivedMessage == null)
            {
                throw new ArgumentNullException(nameof(receivedMessage));
            }

            PrepareContextChange();
            ReceivedMessage = receivedMessage;
            Mode = mode;
        }

        /// <summary>
        /// Modifies the MessagingContext
        /// </summary>
        /// <param name="deliverMessage">The anonymous deliver.</param>
        /// <returns></returns>
        public void ModifyContext(DeliverMessageEnvelope deliverMessage)
        {
            if (deliverMessage == null)
            {
                throw new ArgumentNullException(nameof(deliverMessage));
            }

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
            if (notifyMessage == null)
            {
                throw new ArgumentNullException(nameof(notifyMessage));
            }

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
        PullReceive,
        Deliver,
        Forward,
        Notify
    }
}