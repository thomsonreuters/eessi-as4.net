using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Eu.EDelivery.AS4.UnitTests.Resources;
using Eu.EDelivery.AS4.Xml;
using Xunit;
using Error = Eu.EDelivery.AS4.Model.Core.Error;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;
using Receipt = Eu.EDelivery.AS4.Model.Core.Receipt;
using UserMessage = Eu.EDelivery.AS4.Model.Core.UserMessage;
using static Eu.EDelivery.AS4.UnitTests.Properties.Resources;

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

        protected XmlDocument ExerciseToDocument(AS4Message message)
        {
            return AS4XmlSerializer.ToDocument(new MessagingContext(message), CancellationToken.None);
        }

        protected XmlDocument ExerciseToDocument(MessagingContext context)
        {
            return AS4XmlSerializer.ToDocument(context, CancellationToken.None);
        }

        protected async Task<AS4Message> ExeciseSoapDeserialize(Stream stream)
        {
            // Arrange
            var sut = new SoapEnvelopeSerializer();

            // Act
            return await sut.DeserializeAsync(stream, Constants.ContentTypes.Soap, CancellationToken.None);
        }

        protected async Task ExerciseSoapSerialize(AS4Message message, Stream stream)
        {
            // Arrange
            var sut = new SoapEnvelopeSerializer();

            // Act
            await sut.SerializeAsync(message, stream, CancellationToken.None);
        }

        /// <summary>
        /// Testing if the serializer succeeds
        /// </summary>
        public class SerializeUserMessage : GivenSoapEnvelopeSerializerFacts
        {
            [Fact]
            public async Task ThenDeserializeAS4MessageSucceedsAsync()
            {
                // Arrange
                using (MemoryStream memoryStream = CreateAnonymousAS4Message().ToStream())
                {
                    // Act
                    AS4Message message = await ExeciseSoapDeserialize(memoryStream);

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
                    AS4Message message = await ExeciseSoapDeserialize(memoryStream);

                    // Assert
                    UserMessage userMessage = message.UserMessages.First();
                    Assert.Equal(Constants.Namespaces.TestService, userMessage.CollaborationInfo.Service.Value);
                    Assert.Equal(Constants.Namespaces.TestAction, userMessage.CollaborationInfo.Action);
                    Assert.Equal("eu:edelivery:as4:sampleconversation", userMessage.CollaborationInfo.ConversationId);
                }
            }

            [Fact]
            public async Task ThenParseUserMessagePropertiesParsedCorrectlyAsync()
            {
                using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Samples.UserMessage)))
                {
                    // Act
                    AS4Message message = await ExeciseSoapDeserialize(memoryStream);

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
                    AS4Message message = await ExeciseSoapDeserialize(memoryStream);

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
                    AS4Message message = await ExeciseSoapDeserialize(memoryStream);

                    // Assert
                    UserMessage userMessage = message.UserMessages.First();
                    Assert.Equal("org:eu:europa:as4:example", userMessage.Sender.PartyIds.First().Id);
                    Assert.Equal("Sender", userMessage.Sender.Role);
                }
            }

            [Fact]
            public async Task ThenXmlDocumentContainsOneMessagingHeader()
            {
                // Arrange
                using (var memoryStream = new MemoryStream())
                {
                    AS4Message dummyMessage = CreateAnonymousAS4Message();

                    // Act
                    await ExerciseSoapSerialize(dummyMessage, memoryStream);

                    // Assert
                    AssertXmlDocumentContainsMessagingTag(memoryStream);
                }
            }

            private static AS4Message CreateAnonymousAS4Message()
            {
                return new AS4MessageBuilder().WithUserMessage(CreateAnonymousUserMessage()).Build();
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

        public class SerializeMultihop : GivenSoapEnvelopeSerializerFacts
        {
            [Fact]
            public async Task DeserializeMultiHopSignalMessage()
            {
                // Arrange
                const string contentType =
                    "multipart/related; boundary=\"=-M/sMGEhQK8RBNg/21Nf7Ig==\";\ttype=\"application/soap+xml\"";
                string messageString = Encoding.UTF8.GetString(as4_multihop_message).Replace((char) 0x1F, ' ');
                byte[] messageContent = Encoding.UTF8.GetBytes(messageString);
                using (var messageStream = new MemoryStream(messageContent))
                {
                    var serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());

                    // Act
                    AS4Message actualMessage = await serializer.DeserializeAsync(
                                                   messageStream,
                                                   contentType,
                                                   CancellationToken.None);

                    // Assert
                    Assert.True(actualMessage.IsSignalMessage);
                }
            }

            [Fact]
            public void MultihopUserMessageCreatedWhenSpecifiedInPMode()
            {
                // Arrange
                MessagingContext context = AS4UserMessageWithPMode(MultiHopPMode());

                // Act
                XmlDocument doc = ExerciseToDocument(context);

                // Assert
                var messagingNode = doc.SelectSingleNode("//*[local-name()='Messaging']") as XmlElement;

                Assert.NotNull(messagingNode);
                Assert.Equal(
                    Constants.Namespaces.EbmsNextMsh,
                    messagingNode.GetAttribute("role", Constants.Namespaces.Soap12));
                Assert.True(
                    XmlConvert.ToBoolean(messagingNode.GetAttribute("mustUnderstand", Constants.Namespaces.Soap12)));
            }

            [Fact]
            public async Task ReceiptMessageForMultihopUserMessageIsMultihop()
            {
                MessagingContext context = await SimulatedReceivedUserMessageWithPMode(MultiHopPMode());

                // Create a receipt for this message.
                // Use the CreateReceiptStep, since there is no other way.
                var step = new CreateAS4ReceiptStep();
                StepResult result = await step.ExecuteAsync(context, CancellationToken.None);

                // The result should contain a signalmessage, which is a receipt.
                Assert.True(result.MessagingContext.AS4Message.IsSignalMessage);

                XmlDocument doc = ExerciseToDocument(result.MessagingContext);

                // Following elements should be present:
                // - To element in the wsa namespace
                // - Action element in the wsa namespace
                // - UserElement in the multihop namespace.
                AssertToElementNamespaces(doc);
                Assert.True(ContainsActionElement(doc));
                Assert.True(ContainsUserMessageElement(doc));

                AssertMessagingElementNamespaces(doc);
                AssertIfSignalReferenceUserMessage(context.AS4Message, doc);
                AssertIfSenderAndReceiverAreReversed(context.AS4Message, doc);
            }

            private static void AssertIfSignalReferenceUserMessage(AS4Message as4Message, XmlNode doc)
            {
                string actualRefToMessageId = DeserializeMessagingHeader(doc).SignalMessage.First().MessageInfo.RefToMessageId;
                string expectedUserMessageId = as4Message.PrimaryUserMessage.MessageId;

                Assert.Equal(expectedUserMessageId, actualRefToMessageId);
            }

            [Fact]
            public async Task ErrorMessageForMultiHopUserMessageIsMultiHop()
            {
                // Arrange
                MessagingContext expectedContext = await SimulatedReceivedUserMessageWithPMode(MultiHopPMode());

                AS4Message errorAS4Message = AS4MessageThatReference(expectedContext);

                // Act
                XmlDocument document = ExerciseToDocument(errorAS4Message);

                // Following elements should be present:
                // - To element in the wsa namespace
                // - Action element in the wsa namespace
                // - UserElement in the multihop namespace.
                AssertToElementNamespaces(document);
                Assert.True(ContainsActionElement(document));
                Assert.True(ContainsUserMessageElement(document));

                AssertMessagingElementNamespaces(document);
                AssertIfSenderAndReceiverAreReversed(expectedContext.AS4Message, document);
            }

            private static AS4Message AS4MessageThatReference(MessagingContext expectedContext)
            {
                Error errorSignal =
                    new ErrorBuilder().WithOriginalMessage(expectedContext)
                                      .WithRefToEbmsMessageId(expectedContext.AS4Message.PrimaryUserMessage.MessageId)
                                      .Build();

                return new AS4MessageBuilder().WithSignalMessage(errorSignal).Build();
            }

            private static void AssertToElementNamespaces(XmlNode doc)
            {
                XmlNode toAddressing =
                    doc.SelectSingleNode($@"//*[local-name()='To' and namespace-uri()='{Constants.Namespaces.Addressing}']");

                Assert.NotNull(toAddressing);
                Assert.Equal(Constants.Namespaces.ICloud, toAddressing.InnerText);
            }

            private static void AssertMessagingElementNamespaces(XmlNode doc)
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
                RoutingInput routingInput = GetRoutingInputFrom(doc);

                RoutingInputUserMessage actualMessage = routingInput.UserMessage;
                UserMessage expectedMessage = expectedAS4Message.PrimaryUserMessage;

                Assert.Equal(expectedMessage.Sender.Role, actualMessage.PartyInfo.To.Role);
                Assert.Equal(
                    expectedMessage.Sender.PartyIds.First().Id,
                    actualMessage.PartyInfo.To.PartyId.First().Value);

                Assert.Equal(expectedMessage.Receiver.Role, actualMessage.PartyInfo.From.Role);
                Assert.Equal(
                    expectedMessage.Receiver.PartyIds.First().Id,
                    actualMessage.PartyInfo.From.PartyId.First().Value);
            }

            private static RoutingInput GetRoutingInputFrom(XmlNode doc)
            {
                XmlNode routingInputNode = doc.SelectSingleNode(@"//*[local-name()='RoutingInput']");
                Assert.NotNull(routingInputNode);

                return AS4XmlSerializer.FromString<RoutingInput>(routingInputNode.OuterXml);
            }

            [Fact]
            public async Task CanDeserializeAndReSerializeMultiHopReceipt()
            {
                // Arrange
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(multihopreceipt)))
                {
                    // Act
                    AS4Message multihopReceipt = await ExeciseSoapDeserialize(stream);

                    // Assert
                    Assert.NotNull(multihopReceipt?.PrimarySignalMessage?.MultiHopRouting);

                    // Serialize the Deserialized receipt again, and make sure the RoutingInput element is present and correct.
                    XmlDocument doc = ExerciseToDocument(multihopReceipt);
                    Assert.NotNull(doc.SelectSingleNode(@"//*[local-name()='RoutingInput']"));
                }
            }

            [Fact]
            public async Task ReceiptMessageForNonMultiHopMessageIsNotMultiHop()
            {
                // Arrange
                MessagingContext message = await SimulatedReceivedUserMessageWithPMode(NonMultiHopPMode());

                // Create a receipt for this message.
                // Use the CreateReceiptStep, since there is no other way.
                var step = new CreateAS4ReceiptStep();
                StepResult result = await step.ExecuteAsync(message, CancellationToken.None);

                // The result should contain a signalmessage, which is a receipt.
                Assert.True(result.MessagingContext.AS4Message.IsSignalMessage);

                // Act
                XmlDocument doc = AS4XmlSerializer.ToDocument(result.MessagingContext, CancellationToken.None);

                // Assert
                // No MultiHop related elements may be present:
                // - No Action element in the wsa namespace
                // - No UserElement in the multihop namespace.
                // - No RoutingInput node
                Assert.False(ContainsActionElement(doc));
                Assert.False(ContainsUserMessageElement(doc));
                Assert.Null(doc.SelectSingleNode(@"//*[local-name()='RoutingInput']"));
            }

            private static async Task<MessagingContext> SimulatedReceivedUserMessageWithPMode(
                SendingProcessingMode pmode)
            {
                MessagingContext message = AS4UserMessageWithPMode(pmode);
                ISerializer serializer = SerializerProvider.Default.Get(message.AS4Message.ContentType);

                // Serialize and deserialize the AS4 Message to simulate a received message.
                using (var stream = new MemoryStream())
                {
                    serializer.Serialize(message.AS4Message, stream, CancellationToken.None);
                    stream.Position = 0;

                    AS4Message as4Message = await serializer.DeserializeAsync(stream, message.AS4Message.ContentType, CancellationToken.None);
                    return new MessagingContext(as4Message) {SendingPMode = pmode};
                }
            }

            private static MessagingContext AS4UserMessageWithPMode(SendingProcessingMode pmode)
            {
                var sender = new Party("sender", new PartyId("senderId"));
                var receiver = new Party("rcv", new PartyId("receiverId"));

                AS4Message as4Message =
                    new AS4MessageBuilder().WithUserMessage(new UserMessage {Sender = sender, Receiver = receiver})
                                           .Build();

                return new MessagingContext(as4Message) {SendingPMode = pmode};
            }

            private static SendingProcessingMode MultiHopPMode()
            {
                return new SendingProcessingMode {Id = "multihop-pmode", MessagePackaging = {IsMultiHop = true}};
            }

            private static SendingProcessingMode NonMultiHopPMode()
            {
                return new SendingProcessingMode {Id = "non-multihop-pmode", MessagePackaging = {IsMultiHop = false}};
            }

            private static bool ContainsUserMessageElement(XmlNode doc)
            {
                string xpath = $@"//*[local-name()='UserMessage' and namespace-uri()='{Constants.Namespaces.EbmsMultiHop}']";

                return doc.SelectSingleNode(xpath) != null;
            }

            private static bool ContainsActionElement(XmlNode doc)
            {
                string xpath = $@"//*[local-name()='Action' and namespace-uri()='{Constants.Namespaces.Addressing}']";

                return doc.SelectSingleNode(xpath) != null;
            }
        }
    }

    public class SerializeReceipt : GivenSoapEnvelopeSerializerFacts
    {
        [Fact]
        public void ThenNonRepudiationInfoElementBelongsToCorrectNamespace()
        {
            // Arrange
            AS4Message as4Message =
                new AS4MessageBuilder().WithSignalMessage(CreateReceiptWithNonRepudiationInfo()).Build();

            // Act
            XmlDocument document = ExerciseToDocument(as4Message);

            // Assert
            XmlNode node = document.SelectSingleNode(@"//*[local-name()='NonRepudiationInformation']");
            Assert.NotNull(node);
            Assert.Equal(Constants.Namespaces.EbmsXmlSignals, node.NamespaceURI);
        }

        private static Receipt CreateReceiptWithNonRepudiationInfo()
        {
            var nnri = new ArrayList {new System.Security.Cryptography.Xml.Reference()};

            var receipt = new Receipt
            {
                NonRepudiationInformation = new NonRepudiationInformationBuilder().WithSignedReferences(nnri).Build()
            };

            return receipt;
        }

        [Fact]
        public void ThenRelatedUserMessageElementBelongsToCorrectNamespace()
        {
            // Arrange
            Receipt receipt = CreateReceiptWithRelatedUserMessageInfo();
            AS4Message as4Message = new AS4MessageBuilder().WithSignalMessage(receipt).Build();

            // Act
            XmlDocument document = ExerciseToDocument(as4Message);

            // Assert
            XmlNode node = document.SelectSingleNode(@"//*[local-name()='UserMessage']");
            Assert.NotNull(node);
            Assert.Equal(Constants.Namespaces.EbmsXmlSignals, node.NamespaceURI);
        }

        private static Receipt CreateReceiptWithRelatedUserMessageInfo()
        {
            return new Receipt {UserMessage = new UserMessage("some-usermessage-id")};
        } 
    }
}