using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Builders;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;
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
        private readonly IRegistry _registry;
        private readonly SoapEnvelopeSerializer _serializer;

        public GivenSoapEnvelopeSerializerFacts()
        {
            this._serializer = new SoapEnvelopeSerializer();
            UserMessage userMessage = CreateUserMessage();

            this._message = new AS4MessageBuilder()
                .WithUserMessage(userMessage)
                .Build();

            this._registry = Registry.Instance;
        }

        private UserMessage CreateUserMessage()
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
    }
}