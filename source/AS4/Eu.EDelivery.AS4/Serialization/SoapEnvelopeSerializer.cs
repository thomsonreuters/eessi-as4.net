using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Eu.EDelivery.AS4.Builders.Internal;
using Eu.EDelivery.AS4.Builders.Security;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Resources;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Xml;
using NLog;
using Error = Eu.EDelivery.AS4.Model.Core.Error;
using PullRequest = Eu.EDelivery.AS4.Model.Core.PullRequest;
using Receipt = Eu.EDelivery.AS4.Model.Core.Receipt;
using SignalMessage = Eu.EDelivery.AS4.Model.Core.SignalMessage;
using UserMessage = Eu.EDelivery.AS4.Model.Core.UserMessage;

namespace Eu.EDelivery.AS4.Serialization
{
    /// <summary>
    /// Serialize <see cref="AS4Message" /> to a <see cref="Stream" />
    /// </summary>
    public class SoapEnvelopeSerializer : ISerializer
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private static readonly XmlWriterSettings DefaultXmlWriterSettings = new XmlWriterSettings
        {
            CloseOutput = false,
            Encoding = new UTF8Encoding(false)
        };

        private static readonly XmlReaderSettings DefaultXmlReaderSettings = new XmlReaderSettings
        {
            Async = true,
            CloseInput = false,
            IgnoreComments = true
        };

        public Task SerializeAsync(AS4Message message, Stream stream, CancellationToken cancellationToken)
        {
            return Task.Run(() => this.Serialize(message, stream, cancellationToken), cancellationToken);
        }

        /// <summary>
        /// Serialize SOAP Envelope to a <see cref="Stream" />
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        public void Serialize(AS4Message message, Stream stream, CancellationToken cancellationToken)
        {
            Messaging messagingHeader = CreateMessagingHeader(message);

            var builder = new SoapEnvelopeBuilder();

            XmlNode securityHeader = GetSecurityHeader(message);
            if (securityHeader != null)
            {
                builder.SetSecurityHeader(securityHeader);
            }

            SetMultiHopHeaders(builder, message);

            builder.SetMessagingHeader(messagingHeader);
            builder.SetMessagingBody(message.SigningId.BodySecurityId);

            WriteSoapEnvelopeTo(builder.Build(), stream);
        }

        private static Messaging CreateMessagingHeader(AS4Message message)
        {
            var messagingHeader = new Messaging { SecurityId = message.SigningId.HeaderSecurityId };

            messagingHeader.SignalMessage = AS4Mapper.Map<Xml.SignalMessage[]>(message.SignalMessages);

            messagingHeader.UserMessage = AS4Mapper.Map<Xml.UserMessage[]>(message.UserMessages);

            if (message.IsMultiHopMessage)
            {
                messagingHeader.role = Constants.Namespaces.EbmsNextMsh;
                messagingHeader.mustUnderstand1 = true;
                messagingHeader.mustUnderstand1Specified = true;
            }

            return messagingHeader;
        }

        private static XmlNode GetSecurityHeader(AS4Message message)
        {
            if (message.SecurityHeader.IsSigned == false && message.SecurityHeader.IsEncrypted == false)
            {
                return null;
            }

            return message.SecurityHeader?.GetXml();
        }

        private static void SetMultiHopHeaders(SoapEnvelopeBuilder builder, AS4Message as4Message)
        {
            if (as4Message.IsSignalMessage && as4Message.PrimarySignalMessage.MultiHopRouting != null)
            {
                var to = new To { Role = Constants.Namespaces.EbmsNextMsh };
                builder.SetToHeader(to);

                string actionValue = as4Message.PrimarySignalMessage.GetActionValue();
                builder.SetActionHeader(actionValue);

                var routingInput = new RoutingInput
                {
                    UserMessage = as4Message.PrimarySignalMessage.MultiHopRouting,
                    mustUnderstand = false,
                    mustUnderstandSpecified = true,
                    IsReferenceParameter = true,
                    IsReferenceParameterSpecified = true
                };

                builder.SetRoutingInput(routingInput);
            }
        }

        private static void WriteSoapEnvelopeTo(XmlNode soapEnvelopeDocument, Stream stream)
        {
            using (XmlWriter writer = XmlWriter.Create(stream, DefaultXmlWriterSettings))
            {
                soapEnvelopeDocument.WriteTo(writer);
            }
        }

        /// <summary>
        /// Parser the SOAP message to a <see cref="AS4Message" />
        /// </summary>
        /// <param name="envelopeStream">RequestStream that contains the SOAP Messaging Header</param>
        /// <param name="contentType"></param>
        /// <param name="token"></param>
        /// <returns><see cref="AS4Message" /> that wraps the User and Signal Messages</returns>
        public async Task<AS4Message> DeserializeAsync(Stream envelopeStream, string contentType, CancellationToken token)
        {
            if (envelopeStream == null)
            {
                throw new ArgumentNullException(nameof(envelopeStream));
            }

            using (Stream stream = await CopyEnvelopeStream(envelopeStream).ConfigureAwait(false))
            {
                XmlDocument envelopeDocument = LoadXmlDocument(stream);

                // Sometimes throws 'The 'http://www.w3.org/XML/1998/namespace:lang' attribute is not declared.'
                // ValidateEnvelopeDocument(envelopeDocument);

                stream.Position = 0;

                AS4Message as4Message = AS4Message.Create(envelopeDocument, contentType);

                using (XmlReader reader = XmlReader.Create(stream, DefaultXmlReaderSettings))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        await DeserializeEnvelope(envelopeDocument, as4Message, reader);
                    }
                }

                XmlNode routingInput = envelopeDocument.SelectSingleNode(@"//*[local-name()='RoutingInput']");

                if (routingInput != null)
                {
                    var routing = await AS4XmlSerializer.FromStringAsync<RoutingInput>(routingInput.OuterXml);
                    if (routing != null)
                    {
                        if (as4Message.PrimarySignalMessage != null)
                        {
                            as4Message.PrimarySignalMessage.MultiHopRouting = routing.UserMessage;
                        }
                    }
                }

                return as4Message;
            }
        }

        private static async Task<Stream> CopyEnvelopeStream(Stream envelopeStream)
        {
            int initialCapacity = 4096;

            if (envelopeStream.CanSeek)
            {
                initialCapacity = (int)envelopeStream.Length;
            }

            Stream stream = new MemoryStream(initialCapacity);

            await envelopeStream.CopyToAsync(stream).ConfigureAwait(false);
            stream.Position = 0;

            return stream;
        }

        private void ValidateEnvelopeDocument(XmlDocument envelopeDocument)
        {
            var schemas = new XmlSchemaSet();
            XmlSchema schema = GetEnvelopeSchema();
            schemas.Add(schema);
            envelopeDocument.Schemas = schemas;

            envelopeDocument.Validate(
                (sender, args) => LogManager.GetCurrentClassLogger().Error($"Invalid ebMS Envelope Document: {args.Message}"));

            Logger.Debug("Valid ebMS Envelope Document");
        }

        private static XmlSchema __envelopeSchema;

        private static XmlSchema GetEnvelopeSchema()
        {
            if (__envelopeSchema == null)
            {
                using (var stringReader = new StringReader(Schemas.Soap12))
                {
                    __envelopeSchema = XmlSchema.Read(stringReader, (sender, args) => { });
                }
            }

            return __envelopeSchema;
        }

        private static XmlDocument LoadXmlDocument(Stream stream)
        {
            stream.Position = 0;

            var document = new XmlDocument { PreserveWhitespace = true };
            document.Load(stream);

            return document;
        }

        private static async Task DeserializeEnvelope(XmlDocument envelopeDocument, AS4Message as4Message, XmlReader reader)
        {
            // Try to Deserialize the Messaging-Headers first since here an 
            // XmlSerializer is used to perform the deserialization.  
            // XmlSerializer will be positioned on the next node after deserializing, which
            // causes a next Read to position on the next node.
            // By doing this, it could be possible that the Security node is skipped when we
            // try to deserialize the security header first.
            await DeserializeMessagingHeader(reader, as4Message);

            DeserializeSecurityHeader(reader, envelopeDocument, as4Message);

            DeserializeBody(reader, as4Message);
        }

        private static void DeserializeSecurityHeader(XmlReader reader, XmlDocument envelopeDocument, AS4Message as4Message)
        {
            if (!IsReadersNameSecurityHeader(reader))
            {
                return;
            }

            ISigningStrategy signingStrategy = null;
            IEncryptionStrategy encryptionStrategy = null;

            while (reader.Read() && reader.LocalName.Equals("Security", StringComparison.OrdinalIgnoreCase) == false)
            {
                if (IsReadersNameEncryptedData(reader) && encryptionStrategy == null)
                {
                    encryptionStrategy = EncryptionStrategyBuilder.Create(envelopeDocument).Build();
                }

                if (IsReadersNameSignature(reader))
                {
                    signingStrategy = new SigningStrategyBuilder(envelopeDocument).Build();
                }
            }

            as4Message.SecurityHeader = new SecurityHeader(signingStrategy, encryptionStrategy);
        }

        private static bool IsReadersNameSignature(XmlReader reader)
            => reader.NodeType == XmlNodeType.Element && StringComparer.OrdinalIgnoreCase.Equals(reader.LocalName, "Signature");

        private static bool IsReadersNameEncryptedData(XmlReader reader)
            => reader.NodeType == XmlNodeType.Element && StringComparer.OrdinalIgnoreCase.Equals(reader.LocalName, "EncryptedData");

        private static bool IsReadersNameSecurityHeader(XmlReader reader)
            => StringComparer.OrdinalIgnoreCase.Equals(reader.LocalName, "Security") && reader.IsStartElement();

        private static async Task DeserializeMessagingHeader(XmlReader reader, AS4Message as4Message)
        {
            bool isReadersNameMessaging = StringComparer.OrdinalIgnoreCase.Equals(reader.LocalName, "Messaging")
                                          && reader.NamespaceURI.Equals(Constants.Namespaces.EbmsXmlCore)
                                          && reader.IsStartElement();

            if (!isReadersNameMessaging)
            {
                return;
            }

            var messagingHeader = await AS4XmlSerializer.FromReaderAsync<Messaging>(reader);
            var signalMessages = GetSignalMessagesFromHeader(messagingHeader);
            var userMessages = GetUserMessagesFromHeader(messagingHeader);

            signalMessages.ForEach(s => as4Message.MessageUnits.Add(s));
            userMessages.ForEach(u => as4Message.MessageUnits.Add(u));

            as4Message.SigningId.HeaderSecurityId = messagingHeader.SecurityId;
        }

        private static List<SignalMessage> GetSignalMessagesFromHeader(Messaging messagingHeader)
        {
            if (messagingHeader.SignalMessage == null)
            {
                return new List<SignalMessage>();
            }

            var messages = new List<SignalMessage>();

            foreach (Xml.SignalMessage signalMessage in messagingHeader.SignalMessage)
            {
                AddSignalMessageToList(messages, signalMessage);
            }

            return messages;
        }

        private static void AddSignalMessageToList(ICollection<SignalMessage> signalMessages, Xml.SignalMessage signalMessage)
        {
            if (signalMessage.Error != null)
            {
                signalMessages.Add(AS4Mapper.Map<Error>(signalMessage));
            }

            if (signalMessage.PullRequest != null)
            {
                signalMessages.Add(AS4Mapper.Map<PullRequest>(signalMessage));
            }

            if (signalMessage.Receipt != null)
            {
                signalMessages.Add(AS4Mapper.Map<Receipt>(signalMessage));
            }
        }

        private static List<UserMessage> GetUserMessagesFromHeader(Messaging header)
        {
            return header.UserMessage == null
                ? new List<UserMessage>()
                : AS4Mapper.Map<IEnumerable<UserMessage>>(header.UserMessage).ToList();
        }

        private static void DeserializeBody(XmlReader reader, AS4Message as4Message)
        {
            bool isReadersNameBody = StringComparer.OrdinalIgnoreCase.Equals(reader.LocalName, "Body")
                                     && reader.NamespaceURI.Equals(Constants.Namespaces.Soap12)
                                     && reader.IsStartElement();

            if (isReadersNameBody)
            {
                as4Message.SigningId.BodySecurityId = reader.GetAttribute("Id", Constants.Namespaces.WssSecurityUtility);
            }
        }
    }
}