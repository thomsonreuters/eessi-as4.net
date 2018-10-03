using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Security;
using Eu.EDelivery.AS4.Compression;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.Serialization;
using MimeKit;
using NLog;

namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// Internal AS4 Message between MSH
    /// </summary>
    public class AS4Message : IEquatable<AS4Message>
    {
        private const string GzipContentType = "application/gzip";

        private readonly bool _serializeAsMultiHop;
        private readonly List<Attachment> _attachmens;
        private readonly List<MessageUnit> _messageUnits;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger(); 

        /// <summary>
        /// Prevents a default instance of the <see cref="AS4Message"/> class from being created.
        /// </summary>
        /// <param name="serializeAsMultiHop">if set to <c>true</c> [serialize as multi hop].</param>
        private AS4Message(bool serializeAsMultiHop = false)
        {
            _serializeAsMultiHop = serializeAsMultiHop;
            _attachmens = new List<Attachment>();
            _messageUnits = new List<MessageUnit>();

            ContentType = "application/soap+xml";
            SigningId = new SigningId();
            SecurityHeader = new SecurityHeader();
        }

        public static AS4Message Empty => new AS4Message(serializeAsMultiHop: false);

        public string ContentType { get; private set; }

        public XmlDocument EnvelopeDocument { get; private set; }

        // ReSharper disable once InconsistentNaming
        private bool? __hasMultiHopAttribute;

        /// <summary>
        /// Gets a value indicating whether or not this AS4 Message is a MultiHop message.
        /// </summary>
        public bool IsMultiHopMessage => (__hasMultiHopAttribute ?? false) || (FirstSignalMessage?.IsMultihopSignal ?? false) || _serializeAsMultiHop;

        public IEnumerable<MessageUnit> MessageUnits => _messageUnits.AsReadOnly();

        public IEnumerable<UserMessage> UserMessages => MessageUnits.OfType<UserMessage>();

        public IEnumerable<SignalMessage> SignalMessages => MessageUnits.OfType<SignalMessage>();

        public IEnumerable<Attachment> Attachments => _attachmens.AsReadOnly();

        public SigningId SigningId { get; internal set; }

        public SecurityHeader SecurityHeader { get; internal set; }

        public string[] MessageIds
            => UserMessages.Select(m => m.MessageId).Concat(SignalMessages.Select(m => m.MessageId)).ToArray();

        public UserMessage FirstUserMessage => UserMessages.FirstOrDefault();

        public SignalMessage FirstSignalMessage => SignalMessages.FirstOrDefault();

        public bool IsSignalMessage => PrimaryMessageUnit is SignalMessage;

        public bool HasSignalMessage => MessageUnits.Any(m => m is SignalMessage);

        public bool IsUserMessage => PrimaryMessageUnit is UserMessage;

        public bool HasUserMessage => MessageUnits.Any(m => m is UserMessage);

        public MessageUnit PrimaryMessageUnit => MessageUnits.FirstOrDefault();

        public bool IsSigned => SecurityHeader.IsSigned;

        public bool IsEncrypted => SecurityHeader.IsEncrypted;

        public bool HasAttachments => Attachments?.Any() ?? false;

        public bool IsEmpty => FirstSignalMessage == null && FirstUserMessage == null;

        public bool IsPullRequest => PrimaryMessageUnit is PullRequest;

        /// <summary>
        /// Creates message with a SOAP envelope.
        /// </summary>
        /// <param name="soapEnvelope">The SOAP envelope.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="securityHeader"></param>
        /// <param name="messagingHeader"></param>
        /// <param name="bodyElement"></param>
        ///<remarks>This method should only be used when creating an AS4 Message via deserialization.</remarks>
        /// <returns></returns>
        internal static async Task<AS4Message> CreateAsync(
            XmlDocument soapEnvelope, 
            string contentType, 
            SecurityHeader securityHeader, 
            Xml.Messaging messagingHeader, 
            Xml.Body1 bodyElement)
        {
            if (soapEnvelope == null)
            {
                throw new ArgumentNullException(nameof(soapEnvelope));
            }

            if (String.IsNullOrWhiteSpace(contentType))
            {
                throw new ArgumentException(@"ContentType must be defined.", nameof(contentType));
            }

            if (securityHeader == null)
            {
                throw new ArgumentNullException(nameof(securityHeader));
            }

            if (messagingHeader == null)
            {
                throw new ArgumentNullException(nameof(messagingHeader));
            }

            if (bodyElement == null)
            {
                throw new ArgumentNullException(nameof(bodyElement));
            }

            var result = new AS4Message
            {
                EnvelopeDocument = soapEnvelope,
                ContentType = contentType,
                SecurityHeader = securityHeader
            };

            bool? IsMultihopAttributePresent()
            {
                const string messagingXPath = "/*[local-name()='Envelope']/*[local-name()='Header']/*[local-name()='Messaging']";
                if (result.EnvelopeDocument?.SelectSingleNode(messagingXPath) is XmlElement messagingNode)
                {
                    string role = messagingNode.GetAttribute("role", Constants.Namespaces.Soap12);

                    return !string.IsNullOrWhiteSpace(role) && role.Equals(Constants.Namespaces.EbmsNextMsh);
                }

                return null;
            }

            result.__hasMultiHopAttribute = IsMultihopAttributePresent();

            string bodySecurityId = null;

            if (bodyElement.AnyAttr != null)
            {
                bodySecurityId = bodyElement.AnyAttr.FirstOrDefault(a => a.LocalName == "Id")?.Value;
            }

            result.SigningId = new SigningId(messagingHeader.SecurityId, bodySecurityId);

            result._messageUnits.AddRange(
                await SoapEnvelopeSerializer.GetMessageUnitsFromMessagingHeader(soapEnvelope, messagingHeader));

            return result;
        }

        /// <summary>
        /// Creates message with a <see cref="SendingProcessingMode"/>.
        /// </summary>
        /// <param name="pmode">The pmode.</param>
        /// <returns></returns>
        public static AS4Message Create(SendingProcessingMode pmode)
        {
            return new AS4Message(pmode?.MessagePackaging?.IsMultiHop == true);
        }

        /// <summary>
        /// Creates message with a <see cref="MessageUnit"/> and a optional <see cref="SendingProcessingMode"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="pmode">The pmode.</param>
        /// <returns></returns>
        public static AS4Message Create(MessageUnit message, SendingProcessingMode pmode = null)
        {
            AS4Message as4Message = Create(pmode);

            as4Message.AddMessageUnit(message);

            return as4Message;
        }

        /// <summary>
        /// Creates message with <see cref="MessageUnit"/>'s and a optional <see cref="SendingProcessingMode"/>.
        /// </summary>
        /// <param name="messages">The messages.</param>
        /// <param name="pmode">The pmode.</param>
        /// <returns></returns>
        public static AS4Message Create(IEnumerable<MessageUnit> messages, SendingProcessingMode pmode = null)
        {
            if (messages == null)
            {
                throw new ArgumentNullException(nameof(messages));
            }

            if (messages.Any(m => m is null))
            {
                throw new ArgumentNullException(nameof(messages), @"Message Units contains a 'null' reference");
            }

            AS4Message as4Message = Create(pmode);

            as4Message.AddMessageUnits(messages);

            return as4Message;
        }

        /// <summary>
        /// Gets the primary message identifier.
        /// </summary>
        /// <returns></returns>
        public string GetPrimaryMessageId()
        {
            return IsUserMessage ? FirstUserMessage.MessageId : FirstSignalMessage?.MessageId;
        }

        /// <summary>
        /// Adds a <see cref="MessageUnit"/> to the AS4 Message.
        /// </summary>
        /// <param name="messageUnit">The MessageUnit, which can be a signalmessage or a usermessage.</param>
        /// <remarks>Adding a MessageUnit will cause the EnvelopeDocument property to be set to null, since the 
        /// Envelope Document will no longer be in-sync.</remarks>
        public void AddMessageUnit(MessageUnit messageUnit)
        {
            if (messageUnit == null)
            {
                throw new ArgumentNullException(nameof(messageUnit));
            }

            _messageUnits.Add(messageUnit);
            EnvelopeDocument = null;
        }

        /// <summary>
        /// Updates a given <see cref="MessageUnit"/> in the <see cref="AS4Message"/>
        /// </summary>
        /// <param name="old"></param>
        /// <param name="replacement"></param>
        public void UpdateMessageUnit(MessageUnit old, MessageUnit replacement)
        {
            if (old == null)
            {
                throw new ArgumentNullException(nameof(old));
            }

            if (replacement == null)
            {
                throw new ArgumentNullException(nameof(replacement));
            }

            _messageUnits.Remove(old);
            _messageUnits.Add(replacement);
            EnvelopeDocument = null;
        }

        /// <summary>
        /// Adds <see cref="MessageUnit"/>'s to the AS4 Message.
        /// </summary>
        /// <param name="messageUnits">The MessageUnits, which can be a signalmessage or a usermessage.</param>
        /// <remarks>Adding a MessageUnit will cause the EnvelopeDocument property to be set to null, since the 
        /// Envelope Document will no longer be in-sync.</remarks>
        public void AddMessageUnits(IEnumerable<MessageUnit> messageUnits)
        {
            foreach (MessageUnit messageUnit in messageUnits)
            {
                AddMessageUnit(messageUnit);
            }
        }

        /// <summary>
        /// Clears the MessageUnit collection.
        /// </summary>
        /// <remarks>Clearing the essageUnits will cause the EnvelopeDocument property to be set to null, since the 
        /// Envelope Document will no longer be in-sync.</remarks>
        public void ClearMessageUnits()
        {
            _messageUnits.Clear();
            EnvelopeDocument = null;
        }

        /// <summary>
        /// Determines the size of the message.
        /// </summary>
        /// <returns></returns>
        public long DetermineMessageSize()
        {
            ISerializer serializer = SerializerProvider.Default.Get(ContentType);

            using (var stream = new DetermineSizeStream())
            {
                serializer.Serialize(this, stream, CancellationToken.None);

                return stream.Length;
            }
        }

        /// <summary>
        /// Add Attachment to <see cref="AS4Message" />
        /// </summary>
        /// <param name="attachment"></param>
        /// <exception cref="InvalidOperationException">Throws when there already exists an <see cref="Attachment"/> with the same id</exception>
        public void AddAttachment(Attachment attachment)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            if (!_attachmens.Contains(attachment))
            {
                _attachmens.Add(attachment);
                if (!ContentType.Contains(Constants.ContentTypes.Mime))
                {
                    UpdateContentTypeHeader();
                }
            }
            else
            {
                throw new InvalidOperationException(
                    $"Cannot add attachment because there already exists an 'Attachment' with the Id={attachment.Id}");
            }
        }

        private void UpdateContentTypeHeader()
        {
            string contentTypeString = Constants.ContentTypes.Soap;
            if (Attachments.Any())
            {
                ContentType contentType = new Multipart("related").ContentType;
                contentType.Parameters["type"] = contentTypeString;
                contentType.Charset = Encoding.UTF8.HeaderName.ToLowerInvariant();
                contentTypeString = contentType.ToString();
            }

            ContentType = contentTypeString.Replace("Content-Type: ", string.Empty);
        }

        /// <summary>
        /// Closes the attachments.
        /// </summary>
        public void CloseAttachments()
        {
            foreach (Attachment attachment in Attachments)
            {
                attachment.Content.Dispose();
            }
        }

        /// <summary>
        /// Adds the attachments.
        /// </summary>
        /// <param name="payloads">The payloads.</param>
        /// <param name="retrieval">The retrieval.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">A delegate callback throws an exception.</exception>
        /// <exception cref="InvalidOperationException">Throws when an <see cref="Attachment"/> is being added to a non-UserMessage</exception>
        public async Task AddAttachments(IReadOnlyList<Payload> payloads, Func<Payload, Task<Stream>> retrieval)
        {
            if (payloads == null)
            {
                throw new ArgumentNullException(nameof(payloads));
            }

            if (payloads.Any(p => p is null))
            {
                throw new ArgumentNullException(nameof(payloads), @"Payloads contains a 'null' reference");
            }

            if (retrieval == null)
            {
                throw new ArgumentNullException(nameof(retrieval));
            }

            foreach (Payload payload in payloads)
            {
                Stream content = await retrieval(payload).ConfigureAwait(false);
                var attachment = new Attachment(payload.Id, content, payload.MimeType);
                AddAttachment(attachment);
            }
        }

        /// <summary>
        /// Compresses the Attachments that are part of this AS4 Message and
        /// modifies the Payload-info in the UserMessage to indicate that the attachment 
        /// is compressed.
        /// </summary>
        public void CompressAttachments()
        {
            CompressStrategy
                .ForAS4Message(this)
                .Compress();

            // Since the headers in the message have changed, the EnvelopeDocument
            // is no longer in sync and should be set to null.
            EnvelopeDocument = null;
        }

        /// <summary>
        /// Decompresses the Attachments that are part of this AS4 Message.
        /// </summary>
        public void DecompressAttachments()
        {
            CompressStrategy
                .ForAS4Message(this)
                .Decompress();
        }

        /// <summary>
        /// Removes the given attachment from this message.
        /// </summary>
        /// <param name="tobeRemoved">The tobe removed.</param>
        public void RemoveAttachment(Attachment tobeRemoved)
        {
            if (tobeRemoved == null)
            {
                throw new ArgumentNullException(nameof(tobeRemoved));
            }

            Attachment foundAttachment = _attachmens.FirstOrDefault(a => a == tobeRemoved);
            if (foundAttachment != null)
            {
                _attachmens.Remove(foundAttachment);
                foundAttachment.Content?.Dispose();
            }

            if (!Attachments.Any())
            {
                ContentType = Constants.ContentTypes.Soap;
            }
        }

        /// <summary>
        /// Removes all the attachments present in this message.
        /// </summary>
        public void RemoveAllAttachments()
        {
            CloseAttachments();
            _attachmens.Clear();
            ContentType = Constants.ContentTypes.Soap;
        }

        /// <summary>
        /// Encrypts the AS4 Message using the specified <paramref name="keyEncryptionConfig"/>
        /// and <paramref name="dataEncryptionConfig"/>
        /// </summary>
        /// <param name="keyEncryptionConfig"></param>
        /// <param name="dataEncryptionConfig"></param>
        public void Encrypt(KeyEncryptionConfiguration keyEncryptionConfig, DataEncryptionConfiguration dataEncryptionConfig)
        {
            if (keyEncryptionConfig == null)
            {
                throw new ArgumentNullException(nameof(keyEncryptionConfig));
            }

            if (dataEncryptionConfig == null)
            {
                throw new ArgumentNullException(nameof(dataEncryptionConfig));
            }

            var encryptor =
                EncryptionStrategyBuilder
                    .Create(this, keyEncryptionConfig)
                    .WithDataEncryptionConfiguration(dataEncryptionConfig)
                    .Build();

            SecurityHeader.Encrypt(encryptor);
        }

        /// <summary>
        /// Decrypt the AS4 Message using the specified <paramref name="certificate"/>.
        /// </summary>
        /// <param name="certificate"></param>
        public void Decrypt(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException(nameof(certificate));
            }

            var decryptor = 
                DecryptionStrategyBuilder
                    .Create(this)
                    .WithCertificate(certificate)
                    .Build();

            SecurityHeader.Decrypt(decryptor);
        }

        /// <summary>
        /// Digitally signs the AS4Message using the given <paramref name="signatureConfiguration"/>
        /// </summary>
        /// <param name="signatureConfiguration"></param>
        public void Sign(CalculateSignatureConfig signatureConfiguration)
        {
            if (signatureConfiguration == null)
            {
                throw new ArgumentNullException(nameof(signatureConfiguration));
            }

            SignStrategy signingStrategy = SignStrategy.ForAS4Message(this, signatureConfiguration);
            SecurityHeader.Sign(signingStrategy);
        }

        /// <summary>
        /// Verifies if the digital signature on the AS4 Message is valid.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public bool VerifySignature(VerifySignatureConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var verifier = new SignatureVerificationStrategy(EnvelopeDocument);
            return verifier.VerifySignature(config);
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(AS4Message other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return GetPrimaryMessageId() == other.GetPrimaryMessageId();
        }

        #region Inner DetermineSizeStream class.

        private sealed class DetermineSizeStream : Stream
        {
            private long _length;

            /// <summary>When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.</summary>
            /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream. </param>
            /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream. </param>
            /// <param name="count">The number of bytes to be written to the current stream. </param>
            public override void Write(byte[] buffer, int offset, int count)
            {
                _length += count;
            }

            /// <summary>When overridden in a derived class, gets the length in bytes of the stream.</summary>
            /// <returns>A long value representing the length of the stream in bytes.</returns>
            public override long Length => _length;

            /// <summary>When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.</summary>
            public override void Flush()
            {
                // Do Nothing
            }

            /// <summary>When overridden in a derived class, sets the position within the current stream.</summary>
            /// <returns>The new position within the current stream.</returns>
            /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter. </param>
            /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position. </param>
            public override long Seek(long offset, SeekOrigin origin)
            {
                return -1;
            }

            /// <summary>
            /// When overridden in a derived class, sets the length of the current stream.
            /// </summary>
            /// <param name="value">The desired length of the current stream in bytes.</param>
            /// <exception cref="InvalidOperationException"></exception>
            public override void SetLength(long value)
            {
                throw new InvalidOperationException();
            }

            /// <summary>When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.</summary>
            /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
            /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source. </param>
            /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream. </param>
            /// <param name="count">The maximum number of bytes to be read from the current stream. </param>
            /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            /// <summary>When overridden in a derived class, gets a value indicating whether the current stream supports reading.</summary>
            /// <returns>true if the stream supports reading; otherwise, false.</returns>
            public override bool CanRead => false;

            /// <summary>When overridden in a derived class, gets a value indicating whether the current stream supports seeking.</summary>
            /// <returns>true if the stream supports seeking; otherwise, false.</returns>
            public override bool CanSeek => false;

            /// <summary>When overridden in a derived class, gets a value indicating whether the current stream supports writing.</summary>
            /// <returns>true if the stream supports writing; otherwise, false.</returns>
            public override bool CanWrite => true;

            /// <summary>When overridden in a derived class, gets or sets the position within the current stream.</summary>
            /// <returns>The current position within the stream.</returns>
            public override long Position { get; set; }
        }

        #endregion
    }
}