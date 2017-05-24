using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Common;
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
    public class InternalMessage : IMessage, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InternalMessage" /> class.
        /// Create and Internal Message with empty internals
        /// </summary>
        public InternalMessage()
        {
            AS4Message = null;
            SubmitMessage = new SubmitMessage();
            DeliverMessage = null;
            NotifyMessage = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalMessage" /> class.
        /// Create an Internal Message with a given <see cref="Core.AS4Message" />
        /// </summary>
        /// <param name="as4Message">
        /// </param>
        public InternalMessage(AS4Message as4Message)
        {
            SubmitMessage = null;
            AS4Message = as4Message;
            DeliverMessage = null;
            NotifyMessage = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalMessage" /> class.
        /// Create and Internal Message with a given <see cref="Submit.SubmitMessage" />
        /// </summary>
        /// <param name="submitMessage">
        /// </param>
        public InternalMessage(SubmitMessage submitMessage)
        {
            SubmitMessage = submitMessage;
            AS4Message = null;
            DeliverMessage = null;
            NotifyMessage = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalMessage"/> class.
        /// </summary>
        /// <param name="deliverMessage">The deliver message.</param>
        public InternalMessage(DeliverMessageEnvelope deliverMessage)
        {
            SubmitMessage = null;
            AS4Message = null;
            DeliverMessage = deliverMessage;
            NotifyMessage = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalMessage"/> class.
        /// </summary>
        /// <param name="notifyMessage">The notify message.</param>
        public InternalMessage(NotifyMessageEnvelope notifyMessage)
        {
            SubmitMessage = null;
            AS4Message = null;
            DeliverMessage = null;
            NotifyMessage = notifyMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalMessage"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public InternalMessage(AS4Exception exception)
        {
            Exception = exception;
        }

        // Messages
        public AS4Message AS4Message
        {
            get
            {
                return _as4Message;
            }
            set
            {
                _as4Message = value;

                // TODO: find better approach
                if (_sendingPMode != null)
                {
                    _as4Message.IsMultiHop = SendingPMode.MessagePackaging?.IsMultiHop ?? false;
                }
            }
        }

        public SubmitMessage SubmitMessage { get; set; }

        public DeliverMessageEnvelope DeliverMessage { get; set; }

        public NotifyMessageEnvelope NotifyMessage { get; set; }

        public ReceptionAwareness ReceptionAwareness { get; set; }

        // Exposed Info
        public AS4Exception Exception { get; set; }

        public SendingProcessingMode SendingPMode
        {
            get
            {
                return _sendingPMode;
            }
            set
            {
                _sendingPMode = value;

                // TODO: find better approach
                if (_as4Message != null)
                {
                    _as4Message.IsMultiHop = _sendingPMode?.MessagePackaging?.IsMultiHop ?? false;
                }
            }
        }

        private ReceivingProcessingMode _receivingPMode;

        public ReceivingProcessingMode ReceivingPMode
        {
            get { return _receivingPMode; }
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
        private SendingProcessingMode _sendingPMode;
        private AS4Message _as4Message;

        public string GetReceivingPModeString()
        {
            if (string.IsNullOrWhiteSpace(_receivingPModeString))
            {
                _receivingPModeString = AS4XmlSerializer.ToString(this.ReceivingPMode);
            }

            return _receivingPModeString;
        }


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
        /// Add Attachments to internal <see cref="Core.AS4Message" />
        /// </summary>
        /// <param name="retrieval">Delegate that takes Payload location (string) as argument</param>
        public async Task AddAttachments(Func<Payload, Task<Stream>> retrieval)
        {
            foreach (Payload payload in SubmitMessage.Payloads)
            {
                Attachment attachment = CreateAttachmentFromPayload(payload);
                attachment.Content = await retrieval(payload).ConfigureAwait(false);
                AS4Message.AddAttachment(attachment);
            }
        }

        private static Attachment CreateAttachmentFromPayload(Payload payload)
        {
            return new Attachment(payload.Id) {ContentType = payload.MimeType, Location = payload.Location};
        }


        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            AS4Message?.CloseAttachments();
        }
    }
}