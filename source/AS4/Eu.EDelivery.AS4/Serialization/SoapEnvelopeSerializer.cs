using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Builders.Internal;
using Eu.EDelivery.AS4.Builders.Security;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Mappings;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Resources;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.Singletons;
using NLog;
using Exception = System.Exception;

namespace Eu.EDelivery.AS4.Serialization
{
    /// <summary>
    /// Serialize <see cref="Model.Core.AS4Message" /> to a <see cref="Stream" />
    /// </summary>
    public class SoapEnvelopeSerializer : ISerializer
    {
        private readonly SoapEnvelopeBuilder _builder;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SoapEnvelopeSerializer"/> class. 
        /// Create a new <see cref="SoapEnvelopeSerializer"/>
        /// </summary>
        public SoapEnvelopeSerializer()
        {
            this._builder = new SoapEnvelopeBuilder();
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Serialize SOAP Envelope to a <see cref="Stream" />
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        public void Serialize(Model.Core.AS4Message message, Stream stream, CancellationToken cancellationToken)
        {
            Xml.Messaging messagingHeader = CreateMessagingHeader(message);

            this._builder.BreakDown();
            SetSecurityHeader(message);
            SetMultiHopHeaders(message);

            this._builder.SetMessagingHeader(messagingHeader);
            this._builder.SetMessagingBody(message.SigningId.BodySecurityId);

            WriteSoapEnvelopeTo(stream);
        }

        private Xml.Messaging CreateMessagingHeader(Model.Core.AS4Message message)
        {
            MapInitialization.InitializeMapper();
            var messagingHeader = new Xml.Messaging {SecurityId = message.SigningId.HeaderSecurityId};

            if (message.IsSignalMessage)
                messagingHeader.SignalMessage = AS4Mapper.Map<Xml.SignalMessage[]>(message.SignalMessages);
            else messagingHeader.UserMessage = AS4Mapper.Map<Xml.UserMessage[]>(message.UserMessages);

            if (IsMultiHop(message.SendingPMode))
                messagingHeader.role = Constants.Namespaces.EbmsNextMsh;

            return messagingHeader;
        }

        private bool IsMultiHop(SendingProcessingMode pmode)
        {
            return pmode?.MessagePackaging.IsMultiHop == true;
        }

        private void SetSecurityHeader(AS4Message message)
        {
            if (message.SecurityHeader.IsSigned == false && message.SecurityHeader.IsEncrypted == false) return;

            XmlNode securityNode = message.SecurityHeader?.GetXml();
            if (securityNode != null)
                this._builder.SetSecurityHeader(securityNode);
        }

        private void SetMultiHopHeaders(AS4Message as4Message)
        {
            if (!IsMultiHop(as4Message.SendingPMode) || !as4Message.IsSignalMessage) return;

            SetToHeader();
            SetActionHeader(as4Message);
            SetRoutingInputHeader(as4Message);
        }

        private void SetToHeader()
        {
            var to = new Xml.To {Role = Constants.Namespaces.EbmsNextMsh};
            this._builder.SetToHeader(to);
        }

        private void SetActionHeader(AS4Message as4Message)
        {
            string actionValue = as4Message.PrimarySignalMessage.GetActionValue();
            this._builder.SetActionHeader(actionValue);
        }

        private void SetRoutingInputHeader(AS4Message as4Message)
        {
            var routingInput = new Xml.RoutingInput
            {
                UserMessage = AS4Mapper.Map<Xml.RoutingInputUserMessage>(as4Message.PrimaryUserMessage)
            };

            this._builder.SetRoutingInput(routingInput);
        }

        private void WriteSoapEnvelopeTo(Stream stream)
        {
            XmlDocument soapEnvelopeDoc = this._builder.Build();
            using (XmlWriter writer = XmlWriter.Create(stream, GetXmlWriterSettings()))
                soapEnvelopeDoc.WriteTo(writer);
        }

        private XmlWriterSettings GetXmlWriterSettings()
        {
            return new XmlWriterSettings
            {
                CloseOutput = false,
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
            };
        }

        /// <summary>
        /// Parser the SOAP message to a <see cref="Model.Core.AS4Message" />
        /// </summary>
        /// <param name="envelopeStream">RequestStream that contains the SOAP Messaging Header</param>
        /// <param name="contentType"></param>
        /// <param name="token"></param>
        /// <returns><see cref="Model.Core.AS4Message" /> that wraps the User and Signal Messages</returns>
        public async Task<Model.Core.AS4Message> DeserializeAsync(
            Stream envelopeStream, string contentType, CancellationToken token)
        {
            if (envelopeStream == null)
                throw new ArgumentNullException(nameof(envelopeStream));

            Stream stream = CopyEnvelopeStream(envelopeStream);
            XmlDocument envelopeDocument = LoadXmlDocument(stream);
            ValidateEnvelopeDocument(envelopeDocument);
            stream.Position = 0;

            var as4Message = new Model.Core.AS4Message {ContentType = contentType, EnvelopeDocument = envelopeDocument};

            using (XmlReader reader = XmlReader.Create(stream, GetXmlReaderSettings()))
                while (await reader.ReadAsync().ConfigureAwait(false))
                    DeserializeEnvelope(envelopeDocument, as4Message, reader);

            return as4Message;
        }

        private Stream CopyEnvelopeStream(Stream envelopeStream)
        {
            Stream stream = new MemoryStream();
            envelopeStream.CopyTo(stream);
            stream.Position = 0;

            return stream;
        }

        private void ValidateEnvelopeDocument(XmlDocument envelopeDocument)
        {
            var schemas = new XmlSchemaSet();
            XmlSchema schema = GetEnvelopeSchema();
            schemas.Add(schema);
            envelopeDocument.Schemas = schemas;

            TryValidateEnvelopeDocument(envelopeDocument);

            this._logger.Debug("Valid ebMS Envelope Document");
        }

        private XmlSchema GetEnvelopeSchema()
        {
            using (var stringReader = new StringReader(Schemas.Soap12))
                return XmlSchema.Read(stringReader, (sender, args) => { });
        }

        private void TryValidateEnvelopeDocument(XmlDocument envelopeDocument)
        {
            try
            {
                envelopeDocument.Validate((sender, args)
                    => this._logger.Error($"Invalid ebMS Envelope Document: {args.Message}"));
            }
            catch (XmlSchemaValidationException exception)
            {
                throw ThrowAS4InvalidEnvelopeException(exception);
            }
        }

        private AS4Exception ThrowAS4InvalidEnvelopeException(Exception exception)
        {
            return new AS4ExceptionBuilder()
                .WithInnerException(exception)
                .WithDescription("Invalid ebMS Envelope Document")
                .WithErrorCode(ErrorCode.Ebms0009)
                .WithExceptionType(ExceptionType.InvalidHeader)
                .Build();
        }

        private XmlDocument LoadXmlDocument(Stream stream)
        {
            stream.Position = 0;
            using (XmlReader reader = XmlReader.Create(stream, GetXmlReaderSettings()))
            {
                var document = new XmlDocument();
                document.Load(reader);
                return document;
            }
        }

        private XmlReaderSettings GetXmlReaderSettings()
        {
            return new XmlReaderSettings
            {
                Async = true,
                CloseInput = false,
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };
        }

        private void DeserializeEnvelope(
            XmlDocument envelopeDocument, Model.Core.AS4Message as4Message, XmlReader reader)
        {
            DeserializeSecurityHeader(reader, envelopeDocument, as4Message);
            DeserializeMessagingHeader(reader, as4Message);
            DeserializeBody(reader, as4Message);
        }

        private void DeserializeSecurityHeader(
            XmlReader reader, XmlDocument envelopeDocument, Model.Core.AS4Message as4Message)
        {
            if (!IsReadersNameSecurityHeader(reader)) return;

            ISigningStrategy signingStrategy = null;
            IEncryptionStrategy encryptionStrategy = null;

            while (reader.Read() && !IsReadersNameSecurityHeader(reader))
            {
                if (IsReadersNameEncryptedData(reader) && encryptionStrategy == null)
                    encryptionStrategy = new EncryptionStrategyBuilder(envelopeDocument).Build();

                if (IsReadersNameSignature(reader))
                    signingStrategy = new SigningStrategyBuilder(envelopeDocument).Build();
            }

            as4Message.SecurityHeader = new Model.Core.SecurityHeader(signingStrategy, encryptionStrategy);
        }

        private bool IsReadersNameSignature(XmlReader reader)
            => reader.LocalName == "Signature" && reader.NodeType == XmlNodeType.Element;

        private bool IsReadersNameEncryptedData(XmlReader reader)
            => reader.LocalName == "EncryptedData" && reader.NodeType == XmlNodeType.Element;

        private bool IsReadersNameSecurityHeader(XmlReader reader)
            => reader.LocalName.Equals("Security");

        private void DeserializeMessagingHeader(XmlReader reader, Model.Core.AS4Message as4Message)
        {
            if (!IsReadersNameMessaging(reader)) return;

            var messagingHeader = AS4XmlSerializer.Deserialize<Xml.Messaging>(reader);
            as4Message.SignalMessages = GetSignalMessagesFromHeader(messagingHeader);
            as4Message.UserMessages = GetUserMessagesFromHeader(messagingHeader);
            as4Message.SigningId.HeaderSecurityId = messagingHeader.SecurityId;
        }

        private bool IsReadersNameMessaging(XmlReader reader)
            => reader.LocalName.Equals("Messaging") && IsReadersNamespace(reader);

        private ICollection<Model.Core.SignalMessage> GetSignalMessagesFromHeader(Xml.Messaging messagingHeader)
        {
            var signalMessages = new Collection<Model.Core.SignalMessage>();
            if (messagingHeader.SignalMessage == null) return signalMessages;
            foreach (Xml.SignalMessage signalMessage in messagingHeader.SignalMessage)
                AddSignalMessageToList(signalMessages, signalMessage);

            return signalMessages;
        }

        private void AddSignalMessageToList(
            ICollection<Model.Core.SignalMessage> signalMessages, Xml.SignalMessage signalMessage)
        {
            if (signalMessage.Error != null)
                signalMessages.Add(AS4Mapper.Map<Model.Core.Error>(signalMessage));

            if (signalMessage.PullRequest != null)
                signalMessages.Add(AS4Mapper.Map<Model.Core.PullRequest>(signalMessage));

            if (signalMessage.Receipt != null)
                signalMessages.Add(AS4Mapper.Map<Model.Core.Receipt>(signalMessage));
        }

        private ICollection<Model.Core.UserMessage> GetUserMessagesFromHeader(Xml.Messaging header)
        {
            var userMessages = new List<Model.Core.UserMessage>();
            if (header.UserMessage == null) return userMessages;

            IEnumerable<Model.Core.UserMessage> messages = TryMapUserMessages(header);

            userMessages.AddRange(messages);
            return userMessages;
        }

        private IEnumerable<Model.Core.UserMessage> TryMapUserMessages(Xml.Messaging header)
        {
            try
            {
                return AS4Mapper.Map<IEnumerable<Model.Core.UserMessage>>(header.UserMessage);
            }
            catch (Exception exception) when (exception.GetBaseException() is AS4Exception)
            {
                throw exception.GetBaseException();
            }
        }

        private void DeserializeBody(XmlReader reader, Model.Core.AS4Message as4Message)
        {
            if (!IsReadersNameBody(reader)) return;

            var body = AS4XmlSerializer.Deserialize<Xml.Body>(reader);
            as4Message.SigningId.BodySecurityId = GetBodySecurityId(body);
        }

        private bool IsReadersNameBody(XmlReader reader)
            => reader.LocalName.Equals("Body") && IsReadersNamespace(reader);

        private bool IsReadersNamespace(XmlReader reader)
            => reader.NamespaceURI.Equals(Constants.Namespaces.EbmsXmlCore);

        private string GetBodySecurityId(Xml.Body body)
        {
            string securityId = $"body-{Guid.NewGuid()}";
            if (body == null) return securityId;

            IEnumerator enumerator = body.AnyAttr.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var attribute = (XmlAttribute) enumerator.Current;
                if (attribute.LocalName.Equals("Id"))
                    return attribute.Value;
            }
            return securityId;
        }
    }
}