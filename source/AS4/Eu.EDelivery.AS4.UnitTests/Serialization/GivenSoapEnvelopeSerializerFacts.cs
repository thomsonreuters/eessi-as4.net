using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Builders.Security;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Resources;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.TestUtils;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Eu.EDelivery.AS4.UnitTests.Model;
using Eu.EDelivery.AS4.UnitTests.Resources;
using Eu.EDelivery.AS4.Xml;
using FsCheck;
using FsCheck.Xunit;
using MimeKit;
using Xunit;
using static Eu.EDelivery.AS4.UnitTests.Properties.Resources;
using Error = Eu.EDelivery.AS4.Model.Core.Error;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;
using PullRequest = Eu.EDelivery.AS4.Model.Core.PullRequest;
using Receipt = Eu.EDelivery.AS4.Model.Core.Receipt;
using UserMessage = Eu.EDelivery.AS4.Model.Core.UserMessage;

namespace Eu.EDelivery.AS4.UnitTests.Serialization
{
    /// <summary>
    /// Testing <see cref="SoapEnvelopeSerializer" />
    /// </summary>
    public class GivenSoapEnvelopeSerializerFacts
    {
        /// <summary>
        /// Testing if the serializer succeeds
        /// </summary>
        public class GivenSoapEnvelopeSerializerSucceeds : GivenSoapEnvelopeSerializerFacts
        {
            private const string ServiceNamespace = "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/service";
            private const string ActionNamespace = "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/test";

            [Fact]
            public void ThenMpcAttributeIsCorrectlySerialized()
            {
                var userMessage = new UserMessage("some-message-id") { Mpc = "the-specified-mpc" };
                var as4Message = AS4Message.Create(userMessage);

                using (var messageStream = new MemoryStream())
                {
                    var sut = new SoapEnvelopeSerializer();

                    // Act
                    sut.Serialize(as4Message, messageStream, CancellationToken.None);

                    // Assert
                    messageStream.Position = 0;
                    var xmlDocument = new XmlDocument();
                    xmlDocument.Load(messageStream);

                    var userMessageNode = xmlDocument.SelectSingleNode("//*[local-name()='UserMessage']");
                    Assert.NotNull(userMessageNode);
                    Assert.Equal(userMessage.Mpc, userMessageNode.Attributes["mpc"].InnerText);
                }
            }

            [Fact]
            public async Task ThenDeserializeAS4MessageSucceedsAsync()
            {
                // Arrange
                using (MemoryStream memoryStream = AnonymousAS4UserMessage().ToStream())
                {
                    // Act
                    AS4Message message = await DeserializeAsSoap(memoryStream);

                    // Assert
                    Assert.Equal(1, message.UserMessages.Count());
                }
            }

            [Fact]
            public async void ThenParseUserMessageCollaborationInfoCorrectly()
            {
                using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Samples.UserMessage)))
                {
                    // Act
                    AS4Message message = await DeserializeAsSoap(memoryStream);

                    // Assert
                    UserMessage userMessage = message.UserMessages.First();
                    Assert.Equal(ServiceNamespace, userMessage.CollaborationInfo.Service.Value);
                    Assert.Equal(ActionNamespace, userMessage.CollaborationInfo.Action);
                    Assert.Equal("eu:edelivery:as4:sampleconversation", userMessage.CollaborationInfo.ConversationId);
                }
            }

            [Fact]
            public async Task ThenParseUserMessagePropertiesParsedCorrectlyAsync()
            {
                using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Samples.UserMessage)))
                {
                    // Act
                    AS4Message message = await DeserializeAsSoap(memoryStream);

                    // Assert
                    UserMessage userMessage = message.UserMessages.First();
                    Assert.NotNull(message);
                    Assert.Equal(1, message.UserMessages.Count());
                    Assert.Equal(1472800326948, userMessage.Timestamp.ToUnixTimeMilliseconds());
                }
            }

            [Fact]
            public async void ThenParseUserMessageReceiverCorrectly()
            {
                using (var memoryStream = new MemoryStream(Encoding.UTF32.GetBytes(Samples.UserMessage)))
                {
                    // Act
                    AS4Message message = await DeserializeAsSoap(memoryStream);

                    // Assert
                    UserMessage userMessage = message.UserMessages.First();
                    string receiverId = userMessage.Receiver.PartyIds.First().Id;
                    Assert.Equal("org:holodeckb2b:example:company:B", receiverId);
                    Assert.Equal("Receiver", userMessage.Receiver.Role);
                }
            }

            [Fact]
            public async void ThenParseUserMessageSenderCorrectly()
            {
                using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Samples.UserMessage)))
                {
                    // Act
                    AS4Message message = await DeserializeAsSoap(memoryStream);

                    // Assert
                    UserMessage userMessage = message.UserMessages.First();
                    Assert.Equal("org:eu:europa:as4:example", userMessage.Sender.PartyIds.First().Id);
                    Assert.Equal("Sender", userMessage.Sender.Role);
                }
            }

            private static Task<AS4Message> DeserializeAsSoap(Stream str)
            {
                return new SoapEnvelopeSerializer().DeserializeAsync(str, Constants.ContentTypes.Soap, CancellationToken.None);
            }

            [Fact]
            public async Task AS4UserMessage_ValidatesWithXsdSchema()
            {
                // Arrange
                AS4Message userMessage = AnonymousAS4UserMessage();

                // Act / Assert
                await TestValidEbmsMessageEnvelopeFrom(userMessage);
            }

            [Fact]
            public async Task AS4NRRReceipt_ValidatesWithXsdSchema()
            {
                // Arrange
                AS4Message receiptMessage = AS4Message.Create(new FilledNRRReceipt());

                // Act / Assert
                await TestValidEbmsMessageEnvelopeFrom(receiptMessage);
            }

            [Fact]
            public async Task AS4MultiHopReceipt_ValidatesWithXsdSchema()
            {
                using (var messageStream = new MemoryStream(as4_multihop_message))
                {
                    // Arrange
                    AS4Message receiptMessage = await new MimeMessageSerializer(new SoapEnvelopeSerializer()).DeserializeAsync(
                        inputStream: messageStream,
                        contentType: "multipart/related; boundary=\"=-M/sMGEhQK8RBNg/21Nf7Ig==\";\ttype=\"application/soap+xml\"",
                        cancellationToken: CancellationToken.None);

                    // Act / Assert
                    await TestValidEbmsMessageEnvelopeFrom(receiptMessage);
                }
            }

            [Fact]
            public async Task AS4Error_ValidatesWithXsdSchema()
            {
                // Arrange
                AS4Message errorMessage = AS4Message.Create(new Error("message-id"));

                // Act / Assert
                await TestValidEbmsMessageEnvelopeFrom(errorMessage);
            }

            private static async Task TestValidEbmsMessageEnvelopeFrom(AS4Message message)
            {
                using (var targetStream = new MemoryStream())
                {
                    // Act
                    await new SoapEnvelopeSerializer().SerializeAsync(message, targetStream, CancellationToken.None);

                    // Assert
                    XmlDocument envelope = LoadInEnvelopeDocument(targetStream);
                    Assert.True(IsValidEbmsEnvelope(envelope));
                }
            }

            private static XmlDocument LoadInEnvelopeDocument(Stream targetStream)
            {
                targetStream.Position = 0;

                var envelope = new XmlDocument();
                envelope.Load(targetStream);

                return envelope;
            }

            [Fact]
            public void TestInvalidEnvelope()
            {
                // Arrange
                var envelope = new XmlDocument();
                envelope.LoadXml(Samples.UserMessage.Replace("UserMessage", "InvalidMessage"));

                // Act
                bool isValid = IsValidEbmsEnvelope(envelope);

                // Assert
                Assert.False(isValid);
            }

            private static bool IsValidEbmsEnvelope(XmlDocument envelopeDocument)
            {
                var schemas = new XmlSchemaSet();
                using (var stringReader = new StringReader(Schemas.Soap12))
                {
                    XmlSchema schema = XmlSchema.Read(stringReader, (sender, args) => { });
                    schemas.Add(schema);
                }

                envelopeDocument.Schemas = schemas;

                return ValidateEnvelope(envelopeDocument);
            }

            private static bool ValidateEnvelope(XmlDocument envelopeDocument)
            {
                var isValid = true;
                envelopeDocument.Validate((sender, args) =>
                {
                    isValid = false;
                });

                return isValid;
            }
        }

        /// <summary>
        /// Testing if the AS4Message Succeeds
        /// </summary>
        public class AS4MessageSerializeFacts : GivenAS4MessageFacts
        {
            [Property]
            public void ThenSerializeWithoutAttachmentsReturnsSoapMessage(Guid mpc)
            {
                // Act
                UserMessage userMessage = CreateUserMessage();
                AS4Message message = BuildAS4Message(mpc.ToString(), userMessage);

                using (var soapStream = new MemoryStream())
                {
                    XmlDocument document = SerializeSoapMessage(message, soapStream);
                    XmlNode envelopeElement = document.DocumentElement;

                    // Assert
                    Assert.NotNull(envelopeElement);
                    Assert.Equal(Constants.Namespaces.Soap12, envelopeElement.NamespaceURI);
                }
            }

            [Property]
            public void ThenPullRequestCorrectlySerialized(Guid mpc)
            {
                // Arrange
                UserMessage userMessage = CreateUserMessage();

                AS4Message message = BuildAS4Message(mpc.ToString(), userMessage);

                // Act
                using (var soapStream = new MemoryStream())
                {
                    XmlDocument document = SerializeSoapMessage(message, soapStream);

                    // Assert
                    XmlAttribute mpcAttribute = GetMpcAttribute(document);
                    Assert.NotNull(mpcAttribute);
                    Assert.Equal(mpc.ToString(), mpcAttribute.Value);
                }
            }

            [Property]
            public void ThenSerializeWithAttachmentsReturnsMimeMessage(NonEmptyString messageContents)
            {
                // Arrange
                var attachmentStream = new MemoryStream(Encoding.UTF8.GetBytes(messageContents.Get));
                var attachment = new Attachment("attachment-id") { Content = attachmentStream };

                UserMessage userMessage = CreateUserMessage();

                AS4Message message = AS4Message.Create(userMessage);
                message.AddAttachment(attachment);

                // Act
                AssertMimeMessageIsValid(message);
            }

            private void AssertMimeMessageIsValid(AS4Message message)
            {
                using (var mimeStream = new MemoryStream())
                {
                    MimeMessage mimeMessage = SerializeMimeMessage(message, mimeStream);
                    Stream envelopeStream = mimeMessage.BodyParts.OfType<MimePart>().First().ContentObject.Open();
                    string rawXml = new StreamReader(envelopeStream).ReadToEnd();

                    // Assert
                    Assert.NotNull(rawXml);
                    Assert.Contains("Envelope", rawXml);
                }
            }

            private static XmlAttribute GetMpcAttribute(XmlDocument document)
            {
                const string node = "/s:Envelope/s:Header/ebms:Messaging/ebms:SignalMessage/ebms:PullRequest";
                XmlAttributeCollection attributes = document.SelectXmlNode(node).Attributes;

                return attributes?.Cast<XmlAttribute>().FirstOrDefault(x => x.Name == "mpc");
            }

            [Fact]
            public void ThenSaveToUserMessageCorrectlySerialized()
            {
                // Arrange
                UserMessage userMessage = CreateUserMessage();
                AS4Message message = AS4Message.Create(userMessage);

                // Act
                using (var soapStream = new MemoryStream())
                {
                    message.ContentType = Constants.ContentTypes.Soap;
                    XmlDocument document = SerializeSoapMessage(message, soapStream);

                    // Assert
                    Assert.NotNull(document.DocumentElement);
                    Assert.Contains("Envelope", document.DocumentElement.Name);
                }
            }

            [Fact]
            public void ThenXmlDocumentContainsOneMessagingHeader()
            {
                // Arrange
                using (var memoryStream = new MemoryStream())
                {
                    AS4Message dummyMessage = AnonymousAS4UserMessage();

                    // Act
                    new SoapEnvelopeSerializer().Serialize(dummyMessage, stream: memoryStream, cancellationToken: CancellationToken.None);

                    // Assert
                    AssertXmlDocumentContainsMessagingTag(memoryStream);
                }
            }

            private static void AssertXmlDocumentContainsMessagingTag(Stream stream)
            {
                stream.Position = 0;
                using (var reader = new XmlTextReader(stream))
                {
                    var document = new XmlDocument();
                    document.Load(reader);
                    XmlNodeList nodeList = document.GetElementsByTagName("eb:Messaging");
                    Assert.Equal(1, nodeList.Count);
                }
            }
        }

        private static UserMessage CreateUserMessage()
        {
            return new UserMessage("message-id") { CollaborationInfo = { AgreementReference = new AgreementReference() } };
        }

        private static XmlDocument SerializeSoapMessage(AS4Message message, Stream soapStream)
        {
            ISerializer serializer = new SoapEnvelopeSerializer();
            serializer.Serialize(message, soapStream, CancellationToken.None);

            soapStream.Position = 0;
            var document = new XmlDocument();
            document.Load(soapStream);

            return document;
        }

        private static MimeMessage SerializeMimeMessage(AS4Message message, Stream mimeStream)
        {
            ISerializer serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());
            serializer.Serialize(message, mimeStream, CancellationToken.None);

            message.ContentType = Constants.ContentTypes.Mime;
            mimeStream.Position = 0;

            return MimeMessage.Load(mimeStream);
        }

        private static AS4Message BuildAS4Message(string mpc, UserMessage userMessage)
        {
            AS4Message as4Message = AS4Message.Create(userMessage);
            as4Message.AddMessageUnit(new PullRequest(mpc));

            return as4Message;
        }

        private static AS4Message AnonymousAS4UserMessage()
        {
            return AS4Message.Create(CreateAnonymousUserMessage());
        }

        private static UserMessage CreateAnonymousUserMessage()
        {
            return new UserMessage("message-Id")
            {
                Receiver = new Party("Receiver", new PartyId()),
                Sender = new Party("Sender", new PartyId())
            };
        }
    }

    public class GivenMultiHopSoapEnvelopeSerializerSucceeds
    {
        [Fact]
        public async Task DeserializeMultihopSignalMessage()
        {
            // Arrange
            const string contentType = "multipart/related; boundary=\"=-M/sMGEhQK8RBNg/21Nf7Ig==\";\ttype=\"application/soap+xml\"";
            string messageString = Encoding.UTF8.GetString(as4_multihop_message).Replace((char)0x1F, ' ');
            byte[] messageContent = Encoding.UTF8.GetBytes(messageString);
            using (var messageStream = new MemoryStream(messageContent))
            {
                var serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());

                // Act
                AS4Message actualMessage = await serializer.DeserializeAsync(messageStream, contentType, CancellationToken.None);

                // Assert
                Assert.True(actualMessage.IsSignalMessage);
            }
        }

        [Fact]
        public void MultihopUserMessageCreatedWhenSpecifiedInPMode()
        {
            // Arrange

            AS4Message as4Message = CreateAS4MessageWithPMode(CreateMultiHopPMode());

            // Act
            XmlDocument doc = AS4XmlSerializer.ToSoapEnvelopeDocument(as4Message, CancellationToken.None);

            // Assert
            var messagingNode = doc.SelectSingleNode("//*[local-name()='Messaging']") as XmlElement;

            Assert.NotNull(messagingNode);
            Assert.Equal(Constants.Namespaces.EbmsNextMsh, messagingNode.GetAttribute("role", Constants.Namespaces.Soap12));
            Assert.True(XmlConvert.ToBoolean(messagingNode.GetAttribute("mustUnderstand", Constants.Namespaces.Soap12)));
        }

        [Fact]
        public async void ReceiptMessageForMultihopUserMessageIsMultihop()
        {
            AS4Message as4Message = await CreateReceivedAS4Message(CreateMultiHopPMode());

            var message = new MessagingContext(as4Message, MessagingContextMode.Receive);

            // Create a receipt for this message.
            // Use the CreateReceiptStep, since there is no other way.
            var step = new CreateAS4ReceiptStep();
            StepResult result = await step.ExecuteAsync(message, CancellationToken.None);

            // The result should contain a signalmessage, which is a receipt.
            Assert.True(result.MessagingContext.AS4Message.IsSignalMessage);

            XmlDocument doc = AS4XmlSerializer.ToSoapEnvelopeDocument(result.MessagingContext.AS4Message, CancellationToken.None);

            // Following elements should be present:
            // - To element in the wsa namespace
            // - Action element in the wsa namespace
            // - UserElement in the multihop namespace.
            AssertToElement(doc);
            Assert.True(ContainsActionElement(doc));
            Assert.True(ContainsUserMessageElement(doc));
            AssertUserMessageMessagingElement(as4Message, doc);

            AssertIfSenderAndReceiverAreReversed(as4Message, doc);
        }

        private static void AssertUserMessageMessagingElement(AS4Message as4Message, XmlNode doc)
        {
            AssertMessagingElement(doc);

            string actualRefToMessageId = DeserializeMessagingHeader(doc).SignalMessage.First().MessageInfo.RefToMessageId;
            string expectedUserMessageId = as4Message.PrimaryUserMessage.MessageId;

            Assert.Equal(expectedUserMessageId, actualRefToMessageId);
        }

        [Fact]
        public async Task ErrorMessageForMultihopUserMessageIsMultihop()
        {
            // Arrange
            AS4Message expectedAS4Message = await CreateReceivedAS4Message(CreateMultiHopPMode());

            Error error = new ErrorBuilder()
                .WithRefToEbmsMessageId(expectedAS4Message.PrimaryUserMessage.MessageId)
                .Build();

            error.MultiHopRouting = AS4Mapper.Map<RoutingInputUserMessage>(expectedAS4Message.PrimaryUserMessage);


            AS4Message errorMessage = AS4Message.Create(error);


            // Act
            XmlDocument document = AS4XmlSerializer.ToSoapEnvelopeDocument(errorMessage, CancellationToken.None);

            // Following elements should be present:
            // - To element in the wsa namespace
            // - Action element in the wsa namespace
            // - UserElement in the multihop namespace.
            AssertToElement(document);
            Assert.True(ContainsActionElement(document));
            Assert.True(ContainsUserMessageElement(document));

            AssertMessagingElement(document);
            AssertIfSenderAndReceiverAreReversed(expectedAS4Message, document);
        }

        private static void AssertToElement(XmlNode doc)
        {
            XmlNode toAddressing =
                doc.SelectSingleNode($@"//*[local-name()='To' and namespace-uri()='{Constants.Namespaces.Addressing}']");

            Assert.NotNull(toAddressing);
            Assert.Equal(Constants.Namespaces.ICloud, toAddressing.InnerText);
        }

        [Fact]
        public async Task CanDeserializeAndReSerializeMultiHopReceipt()
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(multihopreceipt)))
            {
                AS4Message multihopReceipt =
                    await SerializerProvider.Default.Get(Constants.ContentTypes.Soap)
                                            .DeserializeAsync(
                                                stream,
                                                Constants.ContentTypes.Soap,
                                                CancellationToken.None);

                Assert.NotNull(multihopReceipt);
                Assert.NotNull(multihopReceipt.PrimarySignalMessage);
                Assert.NotNull(multihopReceipt.PrimarySignalMessage.MultiHopRouting);

                // Serialize the Deserialized receipt again, and make sure the RoutingInput element is present and correct.
                XmlDocument doc = AS4XmlSerializer.ToSoapEnvelopeDocument(multihopReceipt, CancellationToken.None);

                XmlNode routingInput = doc.SelectSingleNode(@"//*[local-name()='RoutingInput']");

                Assert.NotNull(routingInput);
            }
        }

        [Fact]
        public async Task ReceiptMessageForNonMultiHopMessageIsNotMultiHop()
        {
            AS4Message as4Message = await CreateReceivedAS4Message(CreateNonMultiHopPMode());

            var context = new MessagingContext(as4Message, MessagingContextMode.Unknown);

            // Create a receipt for this message.
            // Use the CreateReceiptStep, since there is no other way.
            var step = new CreateAS4ReceiptStep();
            StepResult result = await step.ExecuteAsync(context, CancellationToken.None);

            // The result should contain a signalmessage, which is a receipt.
            Assert.True(result.MessagingContext.AS4Message.IsSignalMessage);

            XmlDocument doc = AS4XmlSerializer.ToSoapEnvelopeDocument(result.MessagingContext.AS4Message, CancellationToken.None);

            // No MultiHop related elements may be present:
            // - No Action element in the wsa namespace
            // - No UserElement in the multihop namespace.
            // - No RoutingInput node
            Assert.False(ContainsActionElement(doc));
            Assert.False(ContainsUserMessageElement(doc));
            Assert.Null(doc.SelectSingleNode(@"//*[local-name()='RoutingInput']"));
        }

        private static bool ContainsUserMessageElement(XmlNode doc)
        {
            return doc.SelectSingleNode($@"//*[local-name()='UserMessage' and namespace-uri()='{Constants.Namespaces.EbmsMultiHop}']") != null;
        }

        private static bool ContainsActionElement(XmlNode doc)
        {
            return doc.SelectSingleNode($@"//*[local-name()='Action' and namespace-uri()='{Constants.Namespaces.Addressing}']") != null;
        }

        private static void AssertMessagingElement(XmlNode doc)
        {
            Messaging messaging = DeserializeMessagingHeader(doc);
            Assert.True(messaging.mustUnderstand1);
            Assert.Equal(Constants.Namespaces.EbmsNextMsh, messaging.role);
        }

        private static Messaging DeserializeMessagingHeader(XmlNode doc)
        {
            XmlNode messagingNode = doc.SelectSingleNode(@"//*[local-name()='Messaging']");
            Assert.NotNull(messagingNode);

            return AS4XmlSerializer.FromString<Messaging>(messagingNode.OuterXml);
        }

        private static void AssertIfSenderAndReceiverAreReversed(AS4Message expectedAS4Message, XmlNode doc)
        {
            XmlNode routingInputNode = doc.SelectSingleNode(@"//*[local-name()='RoutingInput']");
            Assert.NotNull(routingInputNode);
            var routingInput = AS4XmlSerializer.FromString<RoutingInput>(routingInputNode.OuterXml);

            RoutingInputUserMessage actualUserMessage = routingInput.UserMessage;
            UserMessage expectedUserMessage = expectedAS4Message.PrimaryUserMessage;

            Assert.Equal(expectedUserMessage.Sender.Role, actualUserMessage.PartyInfo.To.Role);
            Assert.Equal(
                expectedUserMessage.Sender.PartyIds.First().Id,
                actualUserMessage.PartyInfo.To.PartyId.First().Value);
            Assert.Equal(expectedUserMessage.Receiver.Role, actualUserMessage.PartyInfo.From.Role);
            Assert.Equal(
                expectedUserMessage.Receiver.PartyIds.First().Id,
                actualUserMessage.PartyInfo.From.PartyId.First().Value);
        }

        private static AS4Message CreateAS4MessageWithPMode(SendingProcessingMode pmode)
        {
            var sender = new Party("sender", new PartyId("senderId"));
            var receiver = new Party("rcv", new PartyId("receiverId"));

            return AS4Message.Create(new UserMessage { Sender = sender, Receiver = receiver }, pmode);
        }

        private static async Task<AS4Message> CreateReceivedAS4Message(SendingProcessingMode sendPMode)
        {

            AS4Message message = CreateAS4Message(sendPMode);
            var context = new MessagingContext(message, MessagingContextMode.Receive) { SendingPMode = sendPMode };

            ISerializer serializer = SerializerProvider.Default.Get(message.ContentType);

            // Serialize and deserialize the AS4 Message to simulate a received message.
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(context.AS4Message, stream, CancellationToken.None);
                stream.Position = 0;

                return await serializer.DeserializeAsync(stream, message.ContentType, CancellationToken.None);
            }
        }

        private static AS4Message CreateAS4Message(SendingProcessingMode sendPMode)
        {
            var sender = new Party("sender", new PartyId("senderId"));
            var receiver = new Party("rcv", new PartyId("receiverId"));

            return AS4Message.Create(new UserMessage { Sender = sender, Receiver = receiver }, sendPMode);
        }

        private static SendingProcessingMode CreateMultiHopPMode()
        {
            return new SendingProcessingMode { Id = "multihop-pmode", MessagePackaging = { IsMultiHop = true } };
        }

        private static SendingProcessingMode CreateNonMultiHopPMode()
        {
            return new SendingProcessingMode { Id = "multihop-pmode", MessagePackaging = { IsMultiHop = false } };
        }
    }

    public class GivenReceiptSerializationSucceeds : GivenSoapEnvelopeSerializerFacts
    {
        [Fact]
        public void ThenNonRepudiationInfoElementBelongsToCorrectNamespace()
        {
            var receipt = CreateReceiptWithNonRepudiationInfo();

            var as4Message = AS4Message.Create(receipt);

            XmlDocument document = AS4XmlSerializer.ToSoapEnvelopeDocument(as4Message, CancellationToken.None);

            var node = document.SelectSingleNode(@"//*[local-name()='NonRepudiationInformation']");

            Assert.NotNull(node);
            Assert.Equal(Constants.Namespaces.EbmsXmlSignals, node.NamespaceURI);
        }

        [Fact]
        public void ThenRelatedUserMessageElementBelongsToCorrectNamespace()
        {
            var receipt = CreateReceiptWithRelatedUserMessageInfo();

            var as4Message = AS4Message.Create(receipt);

            XmlDocument document = AS4XmlSerializer.ToSoapEnvelopeDocument(as4Message, CancellationToken.None);

            var node = document.SelectSingleNode(@"//*[local-name()='UserMessage']");

            Assert.NotNull(node);
            Assert.Equal(Constants.Namespaces.EbmsXmlSignals, node.NamespaceURI);
        }

        private static Receipt CreateReceiptWithNonRepudiationInfo()
        {
            var nnri = new[] { new System.Security.Cryptography.Xml.Reference() };

            var receipt = new Receipt
            {
                NonRepudiationInformation = new NonRepudiationInformationBuilder().WithSignedReferences(nnri).Build()
            };

            return receipt;
        }

        private static Receipt CreateReceiptWithRelatedUserMessageInfo()
        {
            var receipt = new Receipt
            {
                UserMessage = new UserMessage("some-usermessage-id")
            };

            return receipt;
        }
    }

    public class GivenReserializationFacts
    {
        [Fact]
        public async Task ReserializedMessageHasUntouchedSoapEnvelope()
        {
            AS4Message deserializedAS4Message = await DeserializeToAS4Message(rssbus_message, @"multipart/related;boundary=""NSMIMEBoundary__e5cfd617-6cec-4276-b190-23f0b25d9d4d"";type=""application/soap+xml"";start=""<_7a711d7c-4d1c-4ce7-ab38-794a01b445e1>""");
            AS4Message reserializedAS4Message = await AS4MessageUtils.SerializeDeserializeAsync(deserializedAS4Message);

            Assert.Equal(deserializedAS4Message.EnvelopeDocument.OuterXml, reserializedAS4Message.EnvelopeDocument.OuterXml);
        }

        [Fact]
        public async Task CanDeserializeEncryptAndSerializeSignedMessageWithUntouchedMessagingHeader()
        {
            // Arrange: retrieve an existing signed AS4 Message and encrypt it. 
            //          Serialize it again to inspect the Soap envelope of the modified message.

            AS4Message deserializedAS4Message = await DeserializeToAS4Message(signed_holodeck_message, @"multipart/related;boundary=""MIMEBoundary_bcb27a6f984295aa9962b01ef2fb3e8d982de76d061ab23f""");

            var originalSecurityHeader = deserializedAS4Message.SecurityHeader.GetXml().CloneNode(deep: true);

            X509Certificate2 encryptionCertificate = new X509Certificate2(certificate_as4, certificate_password);

            // Act: Encrypt the message
            IEncryptionStrategy strategy =
                EncryptionStrategyBuilder.Create(deserializedAS4Message,
                                                 new KeyEncryptionConfiguration(encryptionCertificate))
                                         .Build();

            deserializedAS4Message.SecurityHeader.Encrypt(strategy);

            // Assert: the soap envelope of the encrypted message should not be equal to the
            //         envelope of the original message since there should be modifications in
            //         the security header.
            Assert.NotEqual(originalSecurityHeader.OuterXml, deserializedAS4Message.EnvelopeDocument.OuterXml);

            // Serialize it again; the Soap envelope should remain intact, besides
            // some changes that have been made to the security header.
            var reserializedAS4Message = await AS4MessageUtils.SerializeDeserializeAsync(deserializedAS4Message);

            // Assert: The soap envelopes of both messages should be equal if the 
            //         SecurityHeader is not taken into consideration.

            RemoveSecurityHeaderFromMessageEnvelope(reserializedAS4Message);
            RemoveSecurityHeaderFromMessageEnvelope(deserializedAS4Message);

            Assert.Equal(reserializedAS4Message.EnvelopeDocument.OuterXml, deserializedAS4Message.EnvelopeDocument.OuterXml);
        }

        private static async Task<AS4Message> DeserializeToAS4Message(byte[] content, string contentType)
        {
            // Note that the stream cannot be disposed here, since the AS4Message needs to
            // keep an open reference to it so that it can access the attachments.
            var stream = new MemoryStream(content);

            var serializer = SerializerProvider.Default.Get(contentType);

            return await serializer.DeserializeAsync(stream, contentType, CancellationToken.None);
        }



        private static void RemoveSecurityHeaderFromMessageEnvelope(AS4Message as4Message)
        {
            var headerNode = as4Message.EnvelopeDocument.SelectSingleNode("//*[local-name()='Header']");

            if (headerNode == null)
            {
                return;
            }

            var securityHeader = headerNode.SelectSingleNode("//*[local-name()='Security']");
            if (securityHeader != null)
            {
                headerNode.RemoveChild(securityHeader);
            }
        }
    }
}