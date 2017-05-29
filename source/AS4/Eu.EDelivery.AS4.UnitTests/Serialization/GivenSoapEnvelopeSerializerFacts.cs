using System;
using System.Collections;
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
            public async Task ThenParseUserMessageCollaborationInfoCorrectly()
            {
                // Act
                AS4Message message = await ExerciseSoapDeserializeUserMessage();

                // Assert
                UserMessage userMessage = message.UserMessages.First();
                Assert.Equal(Constants.Namespaces.TestService, userMessage.CollaborationInfo.Service.Value);
                Assert.Equal(Constants.Namespaces.TestAction, userMessage.CollaborationInfo.Action);
                Assert.Equal("eu:edelivery:as4:sampleconversation", userMessage.CollaborationInfo.ConversationId);
            }

            [Fact]
            public async Task ThenParseUserMessagePropertiesParsedCorrectlyAsync()
            {
                // Act 
                AS4Message message = await ExerciseSoapDeserializeUserMessage();

                // Assert
                UserMessage userMessage = message.UserMessages.First();
                Assert.NotNull(message);
                Assert.Equal(1, message.UserMessages.Count);
                Assert.Equal(1472800326948, userMessage.Timestamp.ToUnixTimeMilliseconds());
            }

            [Fact]
            public async Task ThenParseUserMessageReceiverCorrectly()
            {
                // Act
                AS4Message message = await ExerciseSoapDeserializeUserMessage();

                // Assert
                UserMessage userMessage = message.UserMessages.First();
                string receiverId = userMessage.Receiver.PartyIds.First().Id;
                Assert.Equal("org:holodeckb2b:example:company:B", receiverId);
                Assert.Equal("Receiver", userMessage.Receiver.Role);
            }

            [Fact]
            public async Task ThenParseUserMessageSenderCorrectly()
            {
                // Act
                AS4Message message = await ExerciseSoapDeserializeUserMessage();

                // Assert
                UserMessage userMessage = message.UserMessages.First();
                Assert.Equal("org:eu:europa:as4:example", userMessage.Sender.PartyIds.First().Id);
                Assert.Equal("Sender", userMessage.Sender.Role);
            }

            private async Task<AS4Message> ExerciseSoapDeserializeUserMessage()
            {
                using (var userStream = new MemoryStream(Encoding.UTF8.GetBytes(Samples.UserMessage)))
                {
                    return await ExeciseSoapDeserialize(userStream);
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
                    AssertSingleMessagingHeader(memoryStream);
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

            private static void AssertSingleMessagingHeader(Stream stream)
            {
                stream.Position = 0;
                using (var reader = new XmlTextReader(stream))
                {
                    var document = new XmlDocument();
                    document.Load(reader);

                    XmlNodeList messagingHeader = document.GetElementsByTagName("eb:Messaging");
                    Assert.Equal(1, messagingHeader.Count);
                }
            }
        }

        public class SerializeMultihop : GivenSoapEnvelopeSerializerFacts
        {
            [Fact]
            public void IsMultihopUserMessage_WhenSpecifiedInPMode()
            {
                // Arrange
                MessagingContext context = AS4UserMessageWithPMode(MultiHopPMode());

                // Act
                XmlDocument doc = ExerciseToDocument(context);

                // Assert
                var messagingNode = doc.AssertXmlNodeNotNull("Messaging") as XmlElement;

                Assert.Equal(
                    Constants.Namespaces.EbmsNextMsh,
                    messagingNode.GetAttribute("role", Constants.Namespaces.Soap12));
                Assert.True(
                    XmlConvert.ToBoolean(messagingNode.GetAttribute("mustUnderstand", Constants.Namespaces.Soap12)));
            }

            [Fact]
            public async Task IsMultiHopReceipt_IfReceiptForMultihopUserMessage()
            {
                await TestCreateMultiHopSignalMessageFor(
                    async multiHopUserMessage => (await CreateReferencedReceipt(multiHopUserMessage)).AS4Message);
            }

            [Fact]
            public async Task IsMultiHopError_ForUserMessageMultiHop()
            {
                await TestCreateMultiHopSignalMessageFor(
                    multiHopUserMessage => Task.FromResult(CreateReferencedError(multiHopUserMessage)));
            }

            private static AS4Message CreateReferencedError(MessagingContext expectedContext)
            {
                Error errorSignal = 
                    new ErrorBuilder().WithOriginalMessage(expectedContext)
                                      .WithRefToEbmsMessageId(expectedContext.AS4Message.PrimaryUserMessage.MessageId)
                                      .Build();

                return new AS4MessageBuilder().WithSignalMessage(errorSignal).Build();
            }

            private async Task TestCreateMultiHopSignalMessageFor(
                Func<MessagingContext, Task<AS4Message>> createReferencedSignal)
            {
                // Arrange
                MessagingContext multiHopUserMessage = await SimulatedReceivedUserMessageWithPMode(MultiHopPMode());
                AS4Message multiHopSignal = await createReferencedSignal(multiHopUserMessage);

                // Act
                XmlDocument actualDoc = ExerciseToDocument(multiHopSignal);

                // Assert
                AssertToElementNamespaces(actualDoc);
                Assert.True(ContainsActionElementInNamespace(actualDoc));
                Assert.True(ContainsUserMessageElementInMultiHopNamespace(actualDoc));

                AssertMessagingElementNamespaces(actualDoc);
                AssertIfSenderAndReceiverAreReversed(multiHopUserMessage.AS4Message, actualDoc);
                AssertIfSignalReferenceUserMessage(multiHopUserMessage.AS4Message, actualDoc);
            }

            private static void AssertToElementNamespaces(XmlNode doc)
            {
                XmlNode toAddressing =
                    doc.SelectSingleNode($@"//*[local-name()='To' and namespace-uri()='{Constants.Namespaces.Addressing}']");

                Assert.NotNull(toAddressing);
                Assert.Equal(Constants.Namespaces.ICloud, toAddressing.InnerText);
            }

            private static void AssertMessagingElementNamespaces(XmlDocument doc)
            {
                Messaging messaging = DeserializeMessagingHeader(doc);
                Assert.True(messaging.mustUnderstand1);
                Assert.Equal(Constants.Namespaces.EbmsNextMsh, messaging.role);
            }

            private static Messaging DeserializeMessagingHeader(XmlDocument doc)
            {
                XmlNode messagingNode = doc.AssertXmlNodeNotNull("Messaging");
                return AS4XmlSerializer.FromString<Messaging>(messagingNode.OuterXml);
            }

            private static void AssertIfSenderAndReceiverAreReversed(AS4Message expectedAS4Message, XmlDocument doc)
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

            private static void AssertIfSignalReferenceUserMessage(AS4Message as4Message, XmlDocument doc)
            {
                string actualRefToMessageId = DeserializeMessagingHeader(doc).SignalMessage.First().MessageInfo.RefToMessageId;
                string expectedUserMessageId = as4Message.PrimaryUserMessage.MessageId;

                Assert.Equal(expectedUserMessageId, actualRefToMessageId);
            }

            private static RoutingInput GetRoutingInputFrom(XmlDocument doc)
            {
                XmlNode routingInputNode = doc.AssertXmlNodeNotNull("RoutingInput");
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
                    doc.AssertXmlNodeNotNull("RoutingInput");
                }
            }

            [Fact]
            public async Task NonMultiHopReceipt_IfReceiptForNonMultiHop()
            {
                // Arrange
                MessagingContext userMessage = await SimulatedReceivedUserMessageWithPMode(NonMultiHopPMode());
                MessagingContext receipt = await CreateReferencedReceipt(userMessage);

                // Act
                XmlDocument doc = AS4XmlSerializer.ToDocument(receipt, CancellationToken.None);

                // Assert
                Assert.False(ContainsActionElementInNamespace(doc));
                Assert.False(ContainsUserMessageElementInMultiHopNamespace(doc));
                Assert.Null(doc.SelectSingleNode("//*[local-name()='RoutingInput']"));
            }

            private async Task<MessagingContext> SimulatedReceivedUserMessageWithPMode(
                SendingProcessingMode pmode)
            {
                MessagingContext message = AS4UserMessageWithPMode(pmode);

                // Serialize and deserialize the AS4 Message to simulate a received message.
                using (var stream = new MemoryStream())
                {
                    await ExerciseSoapSerialize(message.AS4Message, stream);
                    stream.Position = 0;
                    AS4Message as4Message = await ExeciseSoapDeserialize(stream);

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

            [Fact]
            public async Task TestCreateReferencedReceipt()
            {
                // Arrange
                MessagingContext userMessage = await SimulatedReceivedUserMessageWithPMode(NonMultiHopPMode());

                // Act
                MessagingContext receipt = await CreateReferencedReceipt(userMessage);

                // Assert
                Assert.True(receipt.AS4Message.IsSignalMessage);
            }

            private static async Task<MessagingContext> CreateReferencedReceipt(MessagingContext context)
            {
                // Create a receipt for this message.
                // Use the CreateReceiptStep, since there is no other way.
                var step = new CreateAS4ReceiptStep();
                StepResult result = await step.ExecuteAsync(context, CancellationToken.None);

                return result.MessagingContext;
            }

            private static SendingProcessingMode MultiHopPMode()
            {
                return new SendingProcessingMode {Id = "multihop-pmode", MessagePackaging = {IsMultiHop = true}};
            }

            private static SendingProcessingMode NonMultiHopPMode()
            {
                return new SendingProcessingMode {Id = "non-multihop-pmode", MessagePackaging = {IsMultiHop = false}};
            }

            private static bool ContainsUserMessageElementInMultiHopNamespace(XmlNode doc)
            {
                string xpath =
                    $@"//*[local-name()='UserMessage' and namespace-uri()='{Constants.Namespaces.EbmsMultiHop}']";

                return doc.SelectSingleNode(xpath) != null;
            }

            private static bool ContainsActionElementInNamespace(XmlNode doc)
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
            XmlNode node = document.AssertXmlNodeNotNull("NonRepudiationInformation");
            Assert.Equal(Constants.Namespaces.EbmsXmlSignals, node.NamespaceURI);
        }

        private static Receipt CreateReceiptWithNonRepudiationInfo()
        {
            var references = new ArrayList {new System.Security.Cryptography.Xml.Reference()};

            return new Receipt
            {
                NonRepudiationInformation =
                    new NonRepudiationInformationBuilder().WithSignedReferences(references).Build()
            };
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
            XmlNode node = document.AssertXmlNodeNotNull("UserMessage");
            Assert.Equal(Constants.Namespaces.EbmsXmlSignals, node.NamespaceURI);
        }

        private static Receipt CreateReceiptWithRelatedUserMessageInfo()
        {
            return new Receipt {UserMessage = new UserMessage("some-usermessage-id")};
        } 
    }
}