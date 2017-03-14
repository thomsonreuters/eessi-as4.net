using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Builders;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Resources;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Serialization
{
    /// <summary>
    /// Testing <see cref="SoapEnvelopeSerializer" />
    /// </summary>
    public class GivenSoapEnvelopeSerializerFacts
    {
        private readonly AS4Message _message;
        private readonly SoapEnvelopeSerializer _serializer;

        public GivenSoapEnvelopeSerializerFacts()
        {
            this._serializer = new SoapEnvelopeSerializer();
            UserMessage userMessage = CreateUserMessage();

            this._message = new AS4MessageBuilder()
                .WithUserMessage(userMessage)
                .Build();
        }

        private static UserMessage CreateUserMessage()
        {
            return new UserMessage("message-Id")
            {
                Receiver = new Party("Receiver", new PartyId()),
                Sender = new Party("Sender", new PartyId())
            };
        }

        /// <summary>
        /// Testing if the serializer succeeds
        /// </summary>
        public class GivenSoapEnvelopeSerializerSucceeds : GivenSoapEnvelopeSerializerFacts
        {
            private const string ServiceNamespace =
                "http://docs.oasis-open.org/ebxml-msg/ebMS/v3.0/ns/core/200704/service";

            private const string ActionNamespace = "http://docs.oasis-open.org/ebxml-msg/ebMS/v3.0/ns/core/200704/test";

            [Fact]
            public async Task ThenDeserializeAS4MessageSucceedsAsync()
            {
                // Arrange
                MemoryStream memoryStream = GetSerializedSoapEnvelope();
                const string contentType = Constants.ContentTypes.Soap;
                // Act
                AS4Message message = await this._serializer
                    .DeserializeAsync(memoryStream, contentType, CancellationToken.None);
                // Assert
                Assert.Equal(1, message.UserMessages.Count);
            }

            private MemoryStream GetSerializedSoapEnvelope()
            {
                var memoryStream = new MemoryStream();
                ISerializer serializer = new SoapEnvelopeSerializer();
                serializer.Serialize(base._message, memoryStream, CancellationToken.None);
                memoryStream.Position = 0;
                return memoryStream;
            }

            [Fact]
            public async void ThenParseUserMessageCollaborationInfoCorrectly()
            {
                using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Samples.UserMessage)))
                {
                    // Act
                    AS4Message message = await this._serializer
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
                    AS4Message message = await this._serializer
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
                    AS4Message message = await this._serializer
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
                    AS4Message message = await this._serializer
                        .DeserializeAsync(memoryStream, Constants.ContentTypes.Soap, CancellationToken.None);
                    // Assert
                    UserMessage userMessage = message.UserMessages.First();
                    Assert.Equal("org:eu:europa:as4:example", userMessage.Sender.PartyIds.First().Id);
                    Assert.Equal("Sender", userMessage.Sender.Role);
                }
            }

            [Fact]
            public void ThenXmlDocumentContainsOneMessagingHeader()
            {
                // Arrange
                var memoryStream = new MemoryStream();
                // Act
                this._serializer.Serialize(this._message, memoryStream, CancellationToken.None);
                // Assert
                AssertXmlDocumentContainsMessagingTag(memoryStream);
            }

            private void AssertXmlDocumentContainsMessagingTag(Stream stream)
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
            public void MultihopUserMessageCreatedWhenSpecifiedInPMode()
            {
                var as4Message = CreateAs4Message(CreateMultihopPMode());

                var doc = AS4XmlSerializer.Serialize(as4Message, CancellationToken.None);

                var messagingNode = doc.SelectSingleNode($"//*[local-name()='Messaging']") as XmlElement;

                Assert.NotNull(messagingNode);

                Assert.Equal(Constants.Namespaces.EbmsNextMsh, messagingNode.GetAttribute("role", Constants.Namespaces.Soap12));
                Assert.True(XmlConvert.ToBoolean(messagingNode.GetAttribute("mustUnderstand", Constants.Namespaces.Soap12)));
            }

            [Fact]
            public async void ReceiptMessageForMultihopUserMessageIsMultihop()
            {
                var as4Message = CreateAs4Message(CreateMultihopPMode());

                var message = new InternalMessage()
                {
                    AS4Message = as4Message
                };

                // Create a receipt for this message.
                // Use the CreateReceiptStep, since there is no other way.
                var step = new CreateAS4ReceiptStep();
                var result = await step.ExecuteAsync(message, CancellationToken.None);

                // The result should contain a signalmessage, which is a receipt.
                Assert.True(result.InternalMessage.AS4Message.IsSignalMessage);

                var doc = AS4XmlSerializer.Serialize(result.InternalMessage.AS4Message, CancellationToken.None);

                // Following elements should be present:
                // - To element in the wsa namespace
                // - Action element in the wsa namespace
                // - UserElement in the multihop namespace.
                var toAddressing = doc.SelectSingleNode($@"//*[local-name()='To' and namespace-uri()='{Constants.Namespaces.Addressing}']");

                Assert.NotNull(toAddressing);
                Assert.Equal(Constants.Namespaces.ICloud, toAddressing.InnerText);

                var action = doc.SelectSingleNode($@"//*[local-name()='Action' and namespace-uri()='{Constants.Namespaces.Addressing}']");
                Assert.NotNull(action);

                var userMessage = doc.SelectSingleNode($@"//*[local-name()='UserMessage' and namespace-uri()='{Constants.Namespaces.EbmsMultiHop}']");
                Assert.NotNull(userMessage);

                var messagingNode = doc.SelectSingleNode(@"//*[local-name()='Messaging']");
                Assert.NotNull(messagingNode);
                var messaging = AS4XmlSerializer.Deserialize<Xml.Messaging>(messagingNode.OuterXml);
                Assert.True(messaging.mustUnderstand1);
                Assert.Equal(Constants.Namespaces.EbmsNextMsh, messaging.role);
                Assert.Equal(as4Message.PrimaryUserMessage.MessageId,
                    messaging.SignalMessage.First().MessageInfo.RefToMessageId);

                // Check if sender and receiver are reversed.
                var routingInputNode = doc.SelectSingleNode(@"//*[local-name()='RoutingInput']");
                Assert.NotNull(routingInputNode);
                var routingInput = AS4XmlSerializer.Deserialize<Xml.RoutingInput>(routingInputNode.OuterXml);

                Assert.Equal(as4Message.PrimaryUserMessage.Sender.Role, routingInput.UserMessage.PartyInfo.To.Role);
                Assert.Equal(as4Message.PrimaryUserMessage.Sender.PartyIds.First().Id, routingInput.UserMessage.PartyInfo.To.PartyId.First().Value);
                Assert.Equal(as4Message.PrimaryUserMessage.Receiver.Role, routingInput.UserMessage.PartyInfo.From.Role);
                Assert.Equal(as4Message.PrimaryUserMessage.Receiver.PartyIds.First().Id, routingInput.UserMessage.PartyInfo.From.PartyId.First().Value);
            }

            [Fact]
            public async void ErrorMessageForMultihopUserMessageIsMultihop()
            {
                var as4Message = CreateAs4Message(CreateMultihopPMode());

                var message = new InternalMessage()
                {
                    AS4Message = as4Message,
                    Exception = AS4ExceptionBuilder.WithDescription("AS4 Multihop Error test").WithErrorCode(ErrorCode.Ebms0103).Build()
                };

                // Create a receipt for this message.
                // Use the CreateReceiptStep, since there is no other way.
                var step = new CreateAS4ErrorStep();
                var result = await step.ExecuteAsync(message, CancellationToken.None);

                // The result should contain a signalmessage, which is a receipt.
                Assert.True(result.InternalMessage.AS4Message.IsSignalMessage);

                var doc = AS4XmlSerializer.Serialize(result.InternalMessage.AS4Message, CancellationToken.None);

                // Following elements should be present:
                // - To element in the wsa namespace
                // - Action element in the wsa namespace
                // - UserElement in the multihop namespace.
                var toAddressing = doc.SelectSingleNode($@"//*[local-name()='To' and namespace-uri()='{Constants.Namespaces.Addressing}']");

                Assert.NotNull(toAddressing);
                Assert.Equal(Constants.Namespaces.ICloud, toAddressing.InnerText);

                var action = doc.SelectSingleNode($@"//*[local-name()='Action' and namespace-uri()='{Constants.Namespaces.Addressing}']");
                Assert.NotNull(action);

                var userMessage = doc.SelectSingleNode($@"//*[local-name()='UserMessage' and namespace-uri()='{Constants.Namespaces.EbmsMultiHop}']");
                Assert.NotNull(userMessage);

                var messagingNode = doc.SelectSingleNode(@"//*[local-name()='Messaging']");
                Assert.NotNull(messagingNode);
                var messaging = AS4XmlSerializer.Deserialize<Xml.Messaging>(messagingNode.OuterXml);
                Assert.True(messaging.mustUnderstand1);
                Assert.Equal(Constants.Namespaces.EbmsNextMsh, messaging.role);
                Assert.Equal(as4Message.PrimaryUserMessage.MessageId,
                    messaging.SignalMessage.First().MessageInfo.RefToMessageId);

                // Check if sender and receiver are reversed.
                var routingInputNode = doc.SelectSingleNode(@"//*[local-name()='RoutingInput']");
                Assert.NotNull(routingInputNode);
                var routingInput = AS4XmlSerializer.Deserialize<Xml.RoutingInput>(routingInputNode.OuterXml);

                Assert.Equal(as4Message.PrimaryUserMessage.Sender.Role, routingInput.UserMessage.PartyInfo.To.Role);
                Assert.Equal(as4Message.PrimaryUserMessage.Sender.PartyIds.First().Id, routingInput.UserMessage.PartyInfo.To.PartyId.First().Value);
                Assert.Equal(as4Message.PrimaryUserMessage.Receiver.Role, routingInput.UserMessage.PartyInfo.From.Role);
                Assert.Equal(as4Message.PrimaryUserMessage.Receiver.PartyIds.First().Id, routingInput.UserMessage.PartyInfo.From.PartyId.First().Value);
            }

            private static AS4Message CreateAs4Message(SendingProcessingMode pmode)
            {
                var sender = new Party("sender", new PartyId("senderId"));
                var receiver = new Party("rcv", new PartyId("receiverId"));

                var builder = new AS4MessageBuilder();
                builder.WithSendingPMode(pmode)
                    .WithUserMessage(new UserMessage()
                    {
                        Sender = sender,
                        Receiver = receiver
                    });

                return builder.Build();
            }

            private static SendingProcessingMode CreateMultihopPMode()
            {
                var result = new SendingProcessingMode
                {
                    Id = "multihop-pmode",
                    MessagePackaging = { IsMultiHop = true }
                };

                return result;
            }

        }
    }
}