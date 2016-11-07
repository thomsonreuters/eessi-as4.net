using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Security.Signing;
using MimeKit;

namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// Internal AS4 Message between MSH
    /// </summary>
    public class AS4Message : IMessage
    {
        // Standard Properties
        public string ContentType { get; set; }
        public XmlDocument EnvelopeDocument { get; set; }

        // PModes
        public SendingProcessingMode SendingPMode { get; set; }
        public ReceivingProcessingMode ReceivingPMode { get; set; }

        // AS4 Message
        public ICollection<UserMessage> UserMessages { get; internal set; }
        public ICollection<SignalMessage> SignalMessages { get; internal set; }
        public ICollection<Attachment> Attachments { get; internal set; }

        // Security Properties
        public SigningId SigningId { get; set; }
        public SecurityHeader SecurityHeader { get; set; }

        // Exposed extra info
        public string[] MessageIds => this.UserMessages.Select(m => m.MessageId).Concat(this.SignalMessages.Select(m => m.MessageId)).ToArray();
        public UserMessage PrimaryUserMessage => this.UserMessages.FirstOrDefault();
        public SignalMessage PrimarySignalMessage => this.SignalMessages.FirstOrDefault();

        public bool IsSignalMessage => this.SignalMessages.Count > 0 && this.UserMessages.Count <= 0;
        public bool IsSigned => this.SecurityHeader.IsSigned;
        public bool IsEncrypted => this.SecurityHeader.IsEncrypted;
        public bool HasAttachments => this.Attachments?.Count != 0;
        public bool IsEmpty => this.PrimarySignalMessage == null && this.PrimaryUserMessage == null;

        internal AS4Message()
        {
            this.ContentType = "application/soap+xml";
            this.SigningId = new SigningId();
            this.SecurityHeader = new SecurityHeader();
            this.Attachments = new List<Attachment>();
            this.SignalMessages = new List<SignalMessage>();
            this.UserMessages = new List<UserMessage>();
        }

        /// <summary>
        /// Add Attachment to <see cref="AS4Message" />
        /// </summary>
        /// <param name="attachment"></param>
        public void AddAttachment(Attachment attachment)
        {
            this.Attachments.Add(attachment);
            UpdateContentTypeHeader();
        }

        private void UpdateContentTypeHeader()
        {
            var contentTypeString = "application/soap+xml";
            if (this.Attachments.Count > 0)
            {
                ContentType contentType = new Multipart("related").ContentType;
                contentType.Parameters["type"] = contentTypeString;
                contentType.Charset = Encoding.UTF8.HeaderName.ToLowerInvariant();
                contentTypeString = contentType.ToString();
            }
            this.ContentType = contentTypeString.Replace("Content-Type: ", string.Empty);
        }
    }
}