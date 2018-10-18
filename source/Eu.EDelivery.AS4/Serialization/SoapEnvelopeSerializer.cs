using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using AutoMapper;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Resources;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Streaming;
using Eu.EDelivery.AS4.Xml;
using NLog;
using Error = Eu.EDelivery.AS4.Model.Core.Error;
using NotSupportedException = System.NotSupportedException;
using PullRequest = Eu.EDelivery.AS4.Model.Core.PullRequest;
using Receipt = Eu.EDelivery.AS4.Model.Core.Receipt;
using SignalMessage = Eu.EDelivery.AS4.Model.Core.SignalMessage;
using UserMessage = Eu.EDelivery.AS4.Model.Core.UserMessage;

namespace Eu.EDelivery.AS4.Serialization
{
    /// <summary>
    /// Serialize <see cref="AS4Message" /> to a <see cref="Stream" />
    /// </summary>
    public partial class SoapEnvelopeSerializer : ISerializer
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private static readonly XmlWriterSettings DefaultXmlWriterSettings = new XmlWriterSettings
        {
            CloseOutput = false,
            Encoding = new UTF8Encoding(false)
        };

        /// <summary>
        /// Asynchronously serializes the given <see cref="AS4Message"/> to a given <paramref name="output"/> stream.
        /// </summary>
        /// <param name="message">The message to serialize.</param>
        /// <param name="output">The destination stream to where the message should be written.</param>
        /// <param name="cancellation">The token to control the cancellation of the serialization.</param>
        public Task SerializeAsync(
            AS4Message message, 
            Stream output, 
            CancellationToken cancellation = default(CancellationToken))
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            return Task.Run(() => Serialize(message, output, cancellation), cancellation);
        }

        /// <summary>
        /// Synchronously serializes the given <see cref="AS4Message"/> to a given <paramref name="output"/> stream.
        /// </summary>
        /// <param name="message">The message to serialize.</param>
        /// <param name="output">The destination stream to where the message should be written.</param>
        /// <param name="cancellation">The token to control the cancellation of the serialization.</param>
        public void Serialize(
            AS4Message message, 
            Stream output, 
            CancellationToken cancellation = default(CancellationToken))
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            var builder = new SoapEnvelopeBuilder(message.EnvelopeDocument);

            XmlNode securityHeader = GetSecurityHeader(message);
            if (securityHeader != null)
            {
                builder.SetSecurityHeader(securityHeader);
            }

            if (message.EnvelopeDocument == null)
            {
                SetMultiHopHeaders(builder, message);

                Messaging messagingHeader = CreateMessagingHeader(message);

                builder.SetMessagingHeader(messagingHeader);
                builder.SetMessagingBody(message.SigningId.BodySecurityId);
            }

            using (XmlWriter writer = XmlWriter.Create(output, DefaultXmlWriterSettings))
            {
                builder.Build().WriteTo(writer);
            }
        }

        private static Messaging CreateMessagingHeader(AS4Message message)
        {
            object ToGeneralMessageUnit(MessageUnit u)
            {
                switch (u)
                {
                    case UserMessage _: return AS4Mapper.Map<Xml.UserMessage>(u);
                    case SignalMessage _: return AS4Mapper.Map<Xml.SignalMessage>(u);
                    default:
                        throw new NotSupportedException(
                            $"AS4Message contains unkown MessageUnit of type: {u.GetType()}");
                }
            }

            var messagingHeader = new Messaging
            {
                SecurityId = message.SigningId.HeaderSecurityId,
                MessageUnits = message.MessageUnits.Select(ToGeneralMessageUnit).ToArray()
            };

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
            if (message.SecurityHeader.IsSigned == false 
                && message.SecurityHeader.IsEncrypted == false)
            {
                return null;
            }

            return message.SecurityHeader?.GetXml();
        }

        private static void SetMultiHopHeaders(SoapEnvelopeBuilder builder, AS4Message as4Message)
        {
            if (as4Message.IsSignalMessage && as4Message.FirstSignalMessage.IsMultihopSignal)
            {
                var to = new To { Role = Constants.Namespaces.EbmsNextMsh };
                builder.SetToHeader(to);

                string actionValue = as4Message.FirstSignalMessage.MultihopAction;
                builder.SetActionHeader(actionValue);

                var routingInput = new RoutingInput
                {
                    UserMessage = as4Message.FirstSignalMessage.MultiHopRouting.UnsafeGet,
                    mustUnderstand = false,
                    mustUnderstandSpecified = true,
                    IsReferenceParameter = true,
                    IsReferenceParameterSpecified = true
                };

                builder.SetRoutingInput(routingInput);
            }
        }

        /// <summary>
        /// Asynchronously deserializes the given <paramref name="input"/> stream to an <see cref="AS4Message"/> model.
        /// </summary>
        /// <param name="input">The source stream from where the message should be read.</param>
        /// <param name="contentType">The content type required to correctly deserialize the message into different MIME parts.</param>
        /// <param name="cancellation">The token to control the cancellation of the deserialization.</param>
        public async Task<AS4Message> DeserializeAsync(
            Stream input, 
            string contentType, 
            CancellationToken cancellation = default(CancellationToken))
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            var envelopeDocument = new XmlDocument { PreserveWhitespace = true };
            envelopeDocument.Load(input);

            // Sometimes throws 'The 'http://www.w3.org/XML/1998/namespace:lang' attribute is not declared.'
            // ValidateEnvelopeDocument(envelopeDocument);

            XmlNamespaceManager nsMgr = GetNamespaceManagerForDocument(envelopeDocument);

            SecurityHeader securityHeader = DeserializeSecurityHeader(envelopeDocument, nsMgr);
            Messaging messagingHeader = DeserializeMessagingHeader(envelopeDocument, nsMgr);
            Body1 body = DeserializeBody(envelopeDocument, nsMgr);

            if (messagingHeader == null)
            {
                throw new InvalidMessageException("The envelopeStream does not contain a Messaging element");
            }

            AS4Message as4Message = 
                await AS4Message.CreateAsync(
                    envelopeDocument, 
                    contentType, 
                    securityHeader, 
                    messagingHeader, 
                    body);

            StreamUtilities.MovePositionToStreamStart(input);

            return as4Message;
        }

        private static XmlNamespaceManager GetNamespaceManagerForDocument(XmlDocument doc)
        {
            var nsMgr = new XmlNamespaceManager(doc.NameTable);

            nsMgr.AddNamespace("s", Constants.Namespaces.Soap12);
            nsMgr.AddNamespace("wsse", Constants.Namespaces.WssSecuritySecExt);
            nsMgr.AddNamespace("ds", Constants.Namespaces.XmlDsig);
            nsMgr.AddNamespace("xenc", Constants.Namespaces.XmlEnc);
            nsMgr.AddNamespace("eb3", Constants.Namespaces.EbmsXmlCore);

            return nsMgr;
        }

        private static SecurityHeader DeserializeSecurityHeader(XmlDocument envelopeDocument, XmlNamespaceManager nsMgr)
        {
            if (envelopeDocument.SelectSingleNode("/s:Envelope/s:Header/wsse:Security", nsMgr) 
                is XmlElement securityHeader)
            {
                return new SecurityHeader(securityHeader);
            }

            return new SecurityHeader();

        }

        private static Messaging DeserializeMessagingHeader(XmlDocument document, XmlNamespaceManager nsMgr)
        {
            XmlNode messagingHeader = document.SelectSingleNode("/s:Envelope/s:Header/eb3:Messaging", nsMgr);

            if (messagingHeader == null)
            {
                return null;
            }

            var s = new XmlSerializer(typeof(Messaging), SoapEnvelopeBuilder.MessagingAttributeOverrides);
            return s.Deserialize(new XmlNodeReader(messagingHeader)) as Messaging;
        }
        
        internal static async Task<IEnumerable<MessageUnit>> GetMessageUnitsFromMessagingHeader(
            XmlDocument envelopeDocument, 
            Messaging messagingHeader)
        {
            if (messagingHeader.MessageUnits == null)
            {
                return Enumerable.Empty<MessageUnit>();
            }

            Maybe<RoutingInputUserMessage> routing = await GetRoutingUserMessageFromXml(envelopeDocument);
            MessageUnit ToMessageUnitModel(object u)
            {
                switch (u)
                {
                    case Xml.UserMessage _:
                        return AS4Mapper.Map<UserMessage>(u);
                    case Xml.SignalMessage s:
                        return ConvertSignalMessageFromXml(s, routing);
                    default:
                        throw new NotSupportedException(
                            $"AS4Message has unknown MessageUnit of type: {u.GetType()}");
                }
            }

            return messagingHeader.MessageUnits.Select(ToMessageUnitModel);
        }

        private static async Task<Maybe<RoutingInputUserMessage>> GetRoutingUserMessageFromXml(XmlDocument envelopeDocument)
        {
            XmlNode routingInputTag = envelopeDocument.SelectSingleNode(@"//*[local-name()='RoutingInput']");
            if (routingInputTag != null)
            {
                var routingInput = await AS4XmlSerializer.FromStringAsync<RoutingInput>(routingInputTag.OuterXml);
                if (routingInput?.UserMessage != null)
                {
                    return Maybe.Just(routingInput.UserMessage);
                }
            }

            return Maybe<RoutingInputUserMessage>.Nothing;
        }

        private static SignalMessage ConvertSignalMessageFromXml(Xml.SignalMessage signalMessage, Maybe<RoutingInputUserMessage> routing)
        {
            void AddRouting(IMappingOperationOptions opts)
            {
                routing.Do(r => opts.Items.Add(SignalMessage.RoutingInputKey, r));
            }

            if (signalMessage.Error != null)
            {
                return AS4Mapper.Map<Error>(signalMessage, AddRouting);
            }

            if (signalMessage.PullRequest != null)
            {
                return AS4Mapper.Map<PullRequest>(signalMessage);
            }

            if (signalMessage.Receipt != null)
            {
                return AS4Mapper.Map<Receipt>(signalMessage, AddRouting);
            }

            throw new NotSupportedException("Unable to map Xml.SignalMessage to SignalMessage");
        }

        // ReSharper disable once InconsistentNaming - only used here.
        private static readonly XmlSerializer __bodySerializer = new XmlSerializer(typeof(Body1));

        private static Body1 DeserializeBody(XmlDocument envelopeDocument, XmlNamespaceManager nsMgr)
        {
            XmlNode bodyElement = envelopeDocument.SelectSingleNode("/s:Envelope/s:Body", nsMgr);
            if (bodyElement == null)
            {
                return null;
            }

            object result = __bodySerializer.Deserialize(new XmlNodeReader(bodyElement));
            return result as Body1;
        }
    }
}