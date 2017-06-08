using System.Collections;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Resources;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Eu.EDelivery.AS4.UnitTests.Model;
using Eu.EDelivery.AS4.UnitTests.Resources;
using Eu.EDelivery.AS4.Xml;
using Xunit;
using static Eu.EDelivery.AS4.UnitTests.Properties.Resources;
using Error = Eu.EDelivery.AS4.Model.Core.Error;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;
using Receipt = Eu.EDelivery.AS4.Model.Core.Receipt;
using UserMessage = Eu.EDelivery.AS4.Model.Core.UserMessage;

namespace Eu.EDelivery.AS4.UnitTests.Serialization
{
    /// <summary>
    /// Testing <see cref="SoapEnvelopeSerializer" />
    /// </summary>
    public class GivenSoapEnvelopeSerializerFacts
    {
        private readonly SoapEnvelopeSerializer _serializer;

        public GivenSoapEnvelopeSerializerFacts()
        {
            _serializer = new SoapEnvelopeSerializer();
        }

        public class GivenSerializationIsConsistent
        {
            [Fact(Skip="Skipped on purpose since the Flame Envelope is modified.  Possibly by the .NET SignedXml document; possibly due to how newlines are treated in Windows versus Apple")]
            public async Task DeserializedEnvelopeMustBeIdenticalWithReceivedEnvelope()
            {
                using (var stream = new MemoryStream(as4_flame_envelope))
                {
                    var originalHash = CalculateMD5Hash(stream.ToArray());

                    XmlDocument original = new XmlDocument();
                    original.PreserveWhitespace = true;
                    original.Load(stream);

                    stream.Position = 0;

                    var serializer = new SoapEnvelopeSerializer();
                    var message = await serializer.DeserializeAsync(stream, Constants.ContentTypes.Mime, CancellationToken.None);

                    var deserializedHash = CalculateMD5Hash(Encoding.UTF8.GetBytes(message.EnvelopeDocument.OuterXml));

                    Assert.Equal(message.EnvelopeDocument.OuterXml, original.OuterXml);
                    Assert.Equal(originalHash, deserializedHash);
                }
            }

            private static string CalculateMD5Hash(byte[] input)
            {
                MD5 md5 = MD5.Create();

                byte[] hash = md5.ComputeHash(input);

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Testing if the serializer succeeds
        /// </summary>
        public class GivenSoapEnvelopeSerializerSucceeds : GivenSoapEnvelopeSerializerFacts
        {
            private const string ServiceNamespace = "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/service";
            private const string ActionNamespace = "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/test";

            [Fact]
            public async Task ThenDeserializeAS4MessageSucceedsAsync()
            {
                // Arrange
                using (MemoryStream memoryStream = AnonymousAS4UserMessage().ToStream())
                {
                    // Act
                    AS4Message message = await _serializer
                        .DeserializeAsync(memoryStream, Constants.ContentTypes.Soap, CancellationToken.None);

                    // Assert
                    Assert.Equal(1, message.UserMessages.Count);
                }
            }

            [Fact]
            public async void ThenParseUserMessageCollaborationInfoCorrectly()
            {
                using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Samples.UserMessage)))
                {
                    // Act
                    AS4Message message = await _serializer
                        .DeserializeAsync(memoryStream, Constants.ContentTypes.Soap, CancellationToken.None);

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
                    AS4Message message = await _serializer
                        .DeserializeAsync(memoryStream, Constants.ContentTypes.Soap, CancellationToken.None);

                    // Assert
                    UserMessage userMessage = message.UserMessages.First();
                    Assert.NotNull(message);
                    Assert.Equal(1, message.UserMessages.Count);
                    Assert.Equal(1472800326948, userMessage.Timestamp.ToUnixTimeMilliseconds());
                }
            }

            [Fact]
            public async void ThenParseUserMessageReceiverCorrectly()
            {
                using (var memoryStream = new MemoryStream(Encoding.UTF32.GetBytes(Samples.UserMessage)))
                {
                    // Act
                    AS4Message message = await _serializer
                        .DeserializeAsync(memoryStream, Constants.ContentTypes.Soap, CancellationToken.None);

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
                    AS4Message message = await _serializer
                        .DeserializeAsync(memoryStream, Constants.ContentTypes.Soap, CancellationToken.None);

                    // Assert
                    UserMessage userMessage = message.UserMessages.First();
                    Assert.Equal("org:eu:europa:as4:example", userMessage.Sender.PartyIds.First().Id);
                    Assert.Equal("Sender", userMessage.Sender.Role);
                }
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
                AS4Message receiptMessage = new AS4MessageBuilder().WithSignalMessage(new FilledNRRReceipt()).Build();

                // Act / Assert
                await TestValidEbmsMessageEnvelopeFrom(receiptMessage);
            }

            [Fact]
            public async Task AS4MultiHopReceipt_ValidatesWithXsdSchema()
            {
                using (var messageStream = new MemoryStream(as4_multihop_message))
                {
                    // Arrange
                    AS4Message receiptMessage = await new MimeMessageSerializer(_serializer).DeserializeAsync(
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
                AS4Message errorMessage = new AS4MessageBuilder().WithSignalMessage(new Error("message-id")).Build();

                // Act / Assert
                await TestValidEbmsMessageEnvelopeFrom(errorMessage);
            }

            private async Task TestValidEbmsMessageEnvelopeFrom(AS4Message message)
            {
                using (var targetStream = new MemoryStream())
                {
                    // Act
                    await _serializer.SerializeAsync(message, targetStream, CancellationToken.None);

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
                envelopeDocument.Validate((sender, args) => { isValid = false; });

                return isValid;
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
                _serializer.Serialize(dummyMessage, stream: memoryStream, cancellationToken: CancellationToken.None);

                // Assert
                AssertXmlDocumentContainsMessagingTag(memoryStream);
            }
        }

        private static AS4Message AnonymousAS4UserMessage()
        {
            return new AS4MessageBuilder()
                .WithUserMessage(CreateAnonymousUserMessage())
                .Build();
        }

        private static UserMessage CreateAnonymousUserMessage()
        {
            return new UserMessage("message-Id")
            {
                Receiver = new Party("Receiver", new PartyId()),
                Sender = new Party("Sender", new PartyId())
            };
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
            AS4Message as4Message = CreateAS4Message();
            var context = new MessagingContext(as4Message) {SendingPMode = CreateMultiHopPMode()};

            // Act
            XmlDocument doc = AS4XmlSerializer.ToDocument(context, CancellationToken.None);

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

            var message = new MessagingContext(as4Message);

            // Create a receipt for this message.
            // Use the CreateReceiptStep, since there is no other way.
            var step = new CreateAS4ReceiptStep();
            StepResult result = await step.ExecuteAsync(message, CancellationToken.None);

            // The result should contain a signalmessage, which is a receipt.
            Assert.True(result.MessagingContext.AS4Message.IsSignalMessage);

            XmlDocument doc = AS4XmlSerializer.ToDocument(result.MessagingContext, CancellationToken.None);

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
            
            error.MultiHopRouting = AS4Mapper.Map<RoutingInputUserMessage>(expectedAS4Message?.PrimaryUserMessage);

            AS4Message errorMessage = new AS4MessageBuilder()
                .WithSignalMessage(error)
                .Build();

            var message = new MessagingContext(errorMessage) {SendingPMode = CreateMultiHopPMode()};

            // Act
            XmlDocument document = AS4XmlSerializer.ToDocument(message, CancellationToken.None);

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
                XmlDocument doc = AS4XmlSerializer.ToDocument(new MessagingContext(multihopReceipt), CancellationToken.None);

                XmlNode routingInput = doc.SelectSingleNode(@"//*[local-name()='RoutingInput']");

                Assert.NotNull(routingInput);
            }
        }

        [Fact]
        public async Task ReceiptMessageForNonMultiHopMessageIsNotMultiHop()
        {
            AS4Message as4Message = await CreateReceivedAS4Message(CreateNonMultiHopPMode());

            var message = new MessagingContext(as4Message);

            // Create a receipt for this message.
            // Use the CreateReceiptStep, since there is no other way.
            var step = new CreateAS4ReceiptStep();
            StepResult result = await step.ExecuteAsync(message, CancellationToken.None);

            // The result should contain a signalmessage, which is a receipt.
            Assert.True(result.MessagingContext.AS4Message.IsSignalMessage);

            XmlDocument doc = AS4XmlSerializer.ToDocument(result.MessagingContext, CancellationToken.None);

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
       
        private static async Task<AS4Message> CreateReceivedAS4Message(SendingProcessingMode sendPMode)
        {
            var message = CreateAS4Message();
            var context = new MessagingContext(message) {SendingPMode = sendPMode};

            var serializer = SerializerProvider.Default.Get(message.ContentType);

            // Serialize and deserialize the AS4 Message to simulate a received message.
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(context.AS4Message, stream, CancellationToken.None);
                stream.Position = 0;
                return await serializer.DeserializeAsync(stream, message.ContentType, CancellationToken.None);
            }
        }

        private static AS4Message CreateAS4Message()
        {
            var sender = new Party("sender", new PartyId("senderId"));
            var receiver = new Party("rcv", new PartyId("receiverId"));

            return new AS4MessageBuilder()
                .WithUserMessage(new UserMessage { Sender = sender, Receiver = receiver })
                .Build();
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

            var as4Message = new AS4MessageBuilder().WithSignalMessage(receipt).Build();

            XmlDocument document = AS4XmlSerializer.ToDocument(new MessagingContext(as4Message), CancellationToken.None);

            var node = document.SelectSingleNode(@"//*[local-name()='NonRepudiationInformation']");

            Assert.NotNull(node);
            Assert.Equal(Constants.Namespaces.EbmsXmlSignals, node.NamespaceURI);
        }

        [Fact]
        public void ThenRelatedUserMessageElementBelongsToCorrectNamespace()
        {
            var receipt = CreateReceiptWithRelatedUserMessageInfo();

            var as4Message = new AS4MessageBuilder().WithSignalMessage(receipt).Build();

            XmlDocument document = AS4XmlSerializer.ToDocument(new MessagingContext(as4Message), CancellationToken.None);

            var node = document.SelectSingleNode(@"//*[local-name()='UserMessage']");

            Assert.NotNull(node);
            Assert.Equal(Constants.Namespaces.EbmsXmlSignals, node.NamespaceURI);
        }

        private static Receipt CreateReceiptWithNonRepudiationInfo()
        {
            var nnri = new ArrayList { new System.Security.Cryptography.Xml.Reference() };

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
}