using System;
using System.IO;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Model.Internal
{
    /// <summary>
    /// Canonical Message Format inside the Steps
    /// </summary>
    public class InternalMessage : IMessage
    {
        // Messages
        public AS4Message AS4Message { get; set; }
        public SubmitMessage SubmitMessage { get; set; }
        public DeliverMessageEnvelope DeliverMessage { get; set; }
        public NotifyMessageEnvelope NotifyMessage { get; set; }
        public Entities.ReceptionAwareness ReceptionAwareness { get; set; }

        // Exposed Info
        public AS4Exception Exception { get; set; }
        public string SendingPModeString => AS4XmlSerializer.ToString(this.AS4Message.SendingPMode);
        public string ReceivingPModeString => AS4XmlSerializer.ToString(this.AS4Message.ReceivingPMode);
        public string Prefix => GetPrefix();

        private string GetPrefix()
        {
            string corePrefix = this.AS4Message.PrimaryUserMessage?.MessageId ??
                                this.AS4Message.PrimarySignalMessage?.MessageId;

            string extensionPrefix = this.SubmitMessage.MessageInfo.MessageId ??
                                     this.DeliverMessage?.MessageInfo.MessageId ??                                     
                                     this.NotifyMessage?.MessageInfo.MessageId;

            return $"[{corePrefix ?? extensionPrefix}]";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalMessage"/> class. 
        /// Create and Internal Message with empty internals
        /// </summary>
        public InternalMessage()
        {
            this.AS4Message = new AS4Message();
            this.SubmitMessage = new SubmitMessage();
            this.DeliverMessage = null;
            this.NotifyMessage = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalMessage"/> class. 
        /// Create an Internal Message with a given <see cref="Core.AS4Message"/>
        /// </summary>
        /// <param name="as4Message">
        /// </param>
        public InternalMessage(AS4Message as4Message)
        {
            this.SubmitMessage = new SubmitMessage();
            this.AS4Message = as4Message;
            this.DeliverMessage = null;
            this.NotifyMessage = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalMessage"/> class. 
        /// Create and Internal Message with a given <see cref="Submit.SubmitMessage"/>
        /// </summary>
        /// <param name="submitMessage">
        /// </param>
        public InternalMessage(SubmitMessage submitMessage)
        {
            this.SubmitMessage = submitMessage;
            this.AS4Message = new AS4Message();
            this.DeliverMessage = null;
            this.NotifyMessage = null;
        }

        public InternalMessage(DeliverMessageEnvelope deliverMessage)
        {
            this.SubmitMessage = new SubmitMessage();
            this.AS4Message = new AS4Message();
            this.DeliverMessage = deliverMessage;
            this.NotifyMessage = null;
        }

        public InternalMessage(NotifyMessageEnvelope notifyMessage)
        {
            this.SubmitMessage = new SubmitMessage();
            this.AS4Message = new AS4Message();
            this.DeliverMessage = null;
            this.NotifyMessage = notifyMessage;
        }

        public InternalMessage(AS4Exception exception)
        {
            this.Exception = exception;
        }

        /// <summary>
        /// Add Attachments to internal <see cref="Core.AS4Message" />
        /// </summary>
        /// <param name="retrieval">Delegate that takes Payload location (string) as argument</param>
        public void AddAttachments(Func<Payload, Stream> retrieval)
        {
            foreach (Payload payload in this.SubmitMessage.Payloads)
            {
                Attachment attachment = CreateAttachmentFromPayload(payload);
                attachment.Content = retrieval(payload);
                this.AS4Message.AddAttachment(attachment);
            }
        }

        private static Attachment CreateAttachmentFromPayload(Payload payload)
        {
            return new Attachment(id: payload.Id)
            {
                ContentType = payload.MimeType,
                Location = payload.Location
            };
        }
    }
}