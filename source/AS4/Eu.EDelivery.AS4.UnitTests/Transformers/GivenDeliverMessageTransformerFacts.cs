using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Transformers;
using Xunit;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using MessageProperty = Eu.EDelivery.AS4.Model.Core.MessageProperty;
using Party = Eu.EDelivery.AS4.Model.Core.Party;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    public class GivenDeliverMessageTransformerFacts
    {
		
		[Fact]
        public async Task SucceedsToTransform_IfStreamIsSinglePayloadMessage()
        {
            // Arrange
            var sut = new DeliverMessageTransformer();
            ReceivedMessageEntityMessage message = SinglePayloadMessage();

            // Act
            InternalMessage actualMessage = await sut.TransformAsync(message, CancellationToken.None);

            // Assert
            string expectedMessageId = message.MessageEntity.EbmsMessageId;
            string actualMessageId = actualMessage.DeliverMessage.MessageInfo.MessageId;

            Assert.Equal(expectedMessageId, actualMessageId);
        }

        private static ReceivedMessageEntityMessage SinglePayloadMessage()
        {
            const string contentType =
                "multipart/related; boundary=\"=-PHQq1fuE9QxpIWax7CKj5w==\"; type=\"application/soap+xml\"; charset=\"utf-8\"";

            var messageEntity = new InMessage
            {
                ContentType = contentType,
                EbmsMessageId = "fd85bf2e-2366-408b-b187-010ad63d0070@10.124.29.152",
                EbmsMessageType = MessageType.UserMessage
            };

            return new ReceivedMessageEntityMessage(messageEntity)
            {
                ContentType = contentType,
                RequestStream = new MemoryStream(Properties.Resources.as4_single_payload)
            };
        }
		
        [Fact]
        public async void CreateDeliverMessageForUserMessageThatCorrespondsWithInMessageId()
        {
            var receivedMessage = CreateReceivedInMessageEntity();

            var transformer = new DeliverMessageTransformer();
            var result = await transformer.TransformAsync(receivedMessage, CancellationToken.None);

            Assert.NotNull(result.DeliverMessage);
            Assert.NotEmpty(result.DeliverMessage.DeliverMessage);

            Assert.Equal(receivedMessage.MessageEntity.EbmsMessageId, result.DeliverMessage.MessageInfo.MessageId);
        }
        
        [Fact]
        public async void TransformSucceedsWithValidAgreementRefAsync()
        {
            // Act
            InternalMessage result = await ExecuteTransformerWithDefaultReceivedMessage();

            // Assert
            var deliverMessage =
                AS4XmlSerializer.FromString<DeliverMessage>(
                    Encoding.UTF8.GetString(result.DeliverMessage.DeliverMessage));
            Agreement agreement = deliverMessage.CollaborationInfo.AgreementRef;
            Assert.NotNull(agreement);
            Assert.NotEmpty(agreement.Value);
            Assert.NotNull(agreement.PModeId);
        }

        [Fact]
        public async Task TransformSucceedsWithValidFromPartyAsync()
        {
            // Act
            InternalMessage result = await ExecuteTransformerWithDefaultReceivedMessage();

            // Assert
            var deliverMessage =
                AS4XmlSerializer.FromString<DeliverMessage>(
                    Encoding.UTF8.GetString(result.DeliverMessage.DeliverMessage));
            AS4.Model.Common.Party deliverParty = deliverMessage.PartyInfo.FromParty;
            Assert.NotNull(deliverParty);
            Assert.NotEmpty(deliverParty.Role);
            Assert.NotEmpty(deliverParty.PartyIds);
        }

        [Fact]
        public async Task TransformSucceedsWithValidMessageInfoAsync()
        {
            // Act
            InternalMessage result = await ExecuteTransformerWithDefaultReceivedMessage();

            // Assert
            MessageInfo messageInfo = result.DeliverMessage.MessageInfo;
            Assert.NotEmpty(messageInfo.MessageId);
            Assert.NotEmpty(messageInfo.Mpc);
        }

        [Fact]
        public async Task TransformSucceedsWithValidMessagePropertiesAsync()
        {
            // Act
            InternalMessage result = await ExecuteTransformerWithDefaultReceivedMessage();

            // Assert
            var deliverMessage =
                AS4XmlSerializer.FromString<DeliverMessage>(
                    Encoding.UTF8.GetString(result.DeliverMessage.DeliverMessage));
            AS4.Model.Common.MessageProperty[] props = deliverMessage.MessageProperties;
            Assert.NotNull(props);
            Assert.NotEmpty(props);
        }

        [Fact]
        public async Task TransformSucceedsWithValidServiceAsync()
        {
            // Act
            InternalMessage result = await ExecuteTransformerWithDefaultReceivedMessage();

            // Assert
            var deliverMessage =
                AS4XmlSerializer.FromString<DeliverMessage>(
                    Encoding.UTF8.GetString(result.DeliverMessage.DeliverMessage));
            var service = deliverMessage.CollaborationInfo.Service;
            Assert.NotNull(service);
            Assert.NotEmpty(service.Type);
            Assert.NotEmpty(service.Value);
        }

        [Fact]
        public async Task TransformSucceedsWithvalidToPartyAsync()
        {
            // Act
            InternalMessage result = await ExecuteTransformerWithDefaultReceivedMessage();

            // Assert
            var deliverMessage =
                AS4XmlSerializer.FromString<DeliverMessage>(
                    Encoding.UTF8.GetString(result.DeliverMessage.DeliverMessage));

            AS4.Model.Common.Party deliverParty = deliverMessage.PartyInfo.ToParty;
            Assert.NotNull(deliverParty);
            Assert.NotEmpty(deliverParty.Role);
            Assert.NotEmpty(deliverParty.PartyIds);
        }

        private static async Task<InternalMessage> ExecuteTransformerWithDefaultReceivedMessage()
        {
            var receivedMessage = CreateReceivedInMessageEntity();

            var transformer = new DeliverMessageTransformer();
            return await transformer.TransformAsync(receivedMessage, CancellationToken.None);
        }

        private static ReceivedMessageEntityMessage CreateReceivedInMessageEntity()
        {
            var as4Message = CreateReceivedAS4Message();

            var inMessage = new InMessage
            {
                ContentType = as4Message.ContentType,
                EbmsMessageId = as4Message.UserMessages.ElementAt(1).MessageId,
                Status = InStatus.Received,
                Operation = Operation.ToBeDelivered,
                PMode = AS4XmlSerializer.ToString(as4Message.SendingPMode)
            };

            var receivedStream = new MemoryStream();

            var serializer = SerializerProvider.Default.Get(as4Message.ContentType);
            serializer.Serialize(as4Message, receivedStream, CancellationToken.None);
            receivedStream.Position = 0;

            var receivedMessage = new ReceivedMessageEntityMessage(inMessage)
            {
                ContentType = as4Message.ContentType,
                RequestStream = receivedStream
            };

            return receivedMessage;
        }

        private static AS4Message CreateReceivedAS4Message()
        {
            var pmode = CreateSendingPMode();

            var builder = new AS4MessageBuilder().WithUserMessage(CreateUserMessage(Guid.NewGuid().ToString()))
                .WithUserMessage(CreateUserMessage(Guid.NewGuid().ToString()))
                .WithSendingPMode(pmode);

            return builder.Build();
        }

        private static SendingProcessingMode CreateSendingPMode()
        {
            SendingProcessingMode pmode = new SendingProcessingMode { Id = "some-pmode" };
            return pmode;
        }

        private static UserMessage CreateUserMessage(string userMessageId)
        {
            return new UserMessage(userMessageId)
            {
                Mpc = "mpc",
                CollaborationInfo = CreateCollaborationInfo(),
                Receiver = CreateParty("Receiver", "org:eu:europa:as4:example"),
                Sender = CreateParty("Sender", "org:holodeckb2b:example:company:A"),
                MessageProperties = CreateMessageProperties()
            };
        }

        private static List<MessageProperty> CreateMessageProperties()
        {
            return new List<MessageProperty> { new MessageProperty("Name", "Type", "Value") };
        }

        private static CollaborationInfo CreateCollaborationInfo()
        {
            return new CollaborationInfo
            {
                Action = "StoreMessage",
                Service = {
                    Value = "Test", Type = "org:holodeckb2b:services"
                },
                ConversationId = "org:holodeckb2b:test:conversation",
                AgreementReference = CreateAgreementReference()
            };
        }

        private static AgreementReference CreateAgreementReference()
        {
            return new AgreementReference
            {
                Value = "http://agreements.holodeckb2b.org/examples/agreement0",
                PModeId = "Id"
            };
        }

        private static Party CreateParty(string role, string partyId)
        {
            var partyIds = new List<PartyId> { new PartyId(partyId) };
            return new Party { Role = role, PartyIds = partyIds };
        }
    }
}