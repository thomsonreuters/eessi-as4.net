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
        /// <summary>
        /// Initializes a new instance of the <see cref="AS4Message"/> class.
        /// </summary>
        internal AS4Message()
        {
            ContentType = "application/soap+xml";
            SigningId = new SigningId();
            SecurityHeader = new SecurityHeader();
            Attachments = new List<Attachment>();
            SignalMessages = new List<SignalMessage>();
            UserMessages = new List<UserMessage>();

            SendingPMode = new SendingProcessingMode();
            ReceivingPMode = new ReceivingProcessingMode();
        }

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
        public string[] MessageIds
            => UserMessages.Select(m => m.MessageId).Concat(SignalMessages.Select(m => m.MessageId)).ToArray();

        public UserMessage PrimaryUserMessage => UserMessages.FirstOrDefault();

        public SignalMessage PrimarySignalMessage => SignalMessages.FirstOrDefault();

        public bool IsSignalMessage => SignalMessages.Count > 0;

        public bool IsSigned => SecurityHeader.IsSigned;

        public bool IsEncrypted => SecurityHeader.IsEncrypted;

        public bool HasAttachments => Attachments?.Count != 0;

        public bool IsEmpty => PrimarySignalMessage == null && PrimaryUserMessage == null;

        public bool IsPulling => PrimarySignalMessage is PullRequest;

        /// <summary>
        /// Get the right <see cref="ISendConfiguration" /> for the current <see cref="AS4Message" />.
        /// </summary>
        /// <returns></returns>
        public ISendConfiguration GetSendConfiguration()
        {
            return IsPulling ? (ISendConfiguration)SendingPMode.PullConfiguration : SendingPMode.PushConfiguration;
        }

        /// <summary>
        /// Add Attachment to <see cref="AS4Message" />
        /// </summary>
        /// <param name="attachment"></param>
        public void AddAttachment(Attachment attachment)
        {
            Attachments.Add(attachment);
            UpdateContentTypeHeader();
        }

        private void UpdateContentTypeHeader()
        {
            string contentTypeString = Constants.ContentTypes.Soap;
            if (Attachments.Count > 0)
            {
                ContentType contentType = new Multipart("related").ContentType;
                contentType.Parameters["type"] = contentTypeString;
                contentType.Charset = Encoding.UTF8.HeaderName.ToLowerInvariant();
                contentTypeString = contentType.ToString();
            }

            ContentType = contentTypeString.Replace("Content-Type: ", string.Empty);
        }
    }
}