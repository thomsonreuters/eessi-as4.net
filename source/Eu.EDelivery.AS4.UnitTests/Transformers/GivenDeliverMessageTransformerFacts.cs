using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Transformers;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Eu.EDelivery.AS4.UnitTests.Model;
using Xunit;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using MessageProperty = Eu.EDelivery.AS4.Model.Core.MessageProperty;
using Party = Eu.EDelivery.AS4.Model.Core.Party;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;
using Service = Eu.EDelivery.AS4.Model.Core.Service;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    /// <summary>
    /// Testing <see cref="DeliverMessageTransformer"/>
    /// </summary>
    public class GivenDeliverMessageTransformerFacts
    {
        [Fact]
        public async Task Create_DeliverMessage_From_UserMessage()
        {
            // Arrange
            string partInfoId = $"part-{Guid.NewGuid()}";
            var userMessage = new UserMessage(
                $"user-{Guid.NewGuid()}",
                new CollaborationInfo(
                    new Service($"service-{Guid.NewGuid()}"),
                    $"action-{Guid.NewGuid()}"),
                new Party("Sender", new PartyId($"id-{Guid.NewGuid()}")),
                new Party("Receiver", new PartyId($"id-{Guid.NewGuid()}")),
                new[] { new PartInfo($"cid:{partInfoId}") },
                new MessageProperty[0]);

            AS4Message as4Message = AS4Message.Create(userMessage);
            as4Message.AddAttachment(new Attachment(partInfoId));

            var receivingPMode = new ReceivingProcessingMode { Id = "deliver-pmode" };
            var entity = new InMessage(userMessage.MessageId);
            entity.SetPModeInformation(receivingPMode);

            var fixture = new ReceivedEntityMessage(entity, as4Message.ToStream(), as4Message.ContentType);
            var sut = new DeliverMessageTransformer();

            // Act
            MessagingContext result = await sut.TransformAsync(fixture);

            // Assert
            DeliverMessage deliverMessage = result.DeliverMessage.Message;

            IEnumerable<string> mappingFailures =
                DeliverMessageOriginateFrom(
                    userMessage,
                    receivingPMode,
                    deliverMessage);

            Assert.Empty(mappingFailures);
        }

        private static IEnumerable<string> DeliverMessageOriginateFrom(
            UserMessage user,
            ReceivingProcessingMode receivingPMode,
            DeliverMessage deliver)
        {
            if (user.MessageId != deliver.MessageInfo?.MessageId)
            {
                yield return "MessageId";
            }

            if (user.CollaborationInfo.Service.Value != deliver.CollaborationInfo?.Service?.Value)
            {
                yield return "Service";
            }

            if (user.CollaborationInfo.Action != deliver.CollaborationInfo?.Action)
            {
                yield return "Action";
            }

            if (user.Sender.PrimaryPartyId != deliver.PartyInfo?.FromParty?.PartyIds?.FirstOrDefault()?.Id)
            {
                yield return "FromParty";
            }

            if (user.Receiver.PrimaryPartyId != deliver.PartyInfo?.ToParty?.PartyIds?.FirstOrDefault()?.Id)
            {
                yield return "ToParty";
            }

            if (receivingPMode.Id != deliver.CollaborationInfo?.AgreementRef?.PModeId)
            {
                yield return "PModeId";
            }
        }

        [Fact]
        public async Task FailsToTransform_IfNoUserMessageCanBeFound()
        {
            // Arrange
            var sut = new DeliverMessageTransformer();
            ReceivedEntityMessage receivedMessage = CreateReceivedMessage(receivedInMessageId: "ignored id", as4Message: AS4Message.Empty);

            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(
                () => sut.TransformAsync(receivedMessage));
        }

        [Fact]
        public async Task FailsToTransform_IfInvalidMessageEntityHasGiven()
        {
            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(
                () => new DeliverMessageTransformer().TransformAsync(message: null));
        }

        [Fact]
        public async Task TransformRemovesUnnecessaryAttachments()
        {
            // Arrange
            const string expectedId = "usermessage-id";
            const string expectedUri = "expected-attachment-uri";

            var user = new UserMessage(expectedId, new PartInfo("cid:" + expectedUri));
            AS4Message message = AS4Message.Create(user);
            message.AddAttachment(FilledAttachment(expectedUri));
            message.AddAttachment(FilledAttachment());
            message.AddAttachment(FilledAttachment());

            // Act
            MessagingContext actualMessage = await ExerciseTransform(expectedId, message);

            // Assert
            Assert.Single(actualMessage.DeliverMessage.Attachments);
        }

        private static Attachment FilledAttachment(string attachmentId = null)
        {
            return new Attachment(
                id: attachmentId ?? Guid.NewGuid().ToString(),
                content: new MemoryStream(Encoding.UTF8.GetBytes("serialize me!")),
                contentType: "text/plain");
        }

        private static async Task<MessagingContext> ExerciseTransform(string expectedId, AS4Message as4Message)
        {
            ReceivedEntityMessage receivedMessage = CreateReceivedMessage(receivedInMessageId: expectedId, as4Message: as4Message);
            var sut = new DeliverMessageTransformer();

            return await sut.TransformAsync(receivedMessage);
        }

        private static ReceivedEntityMessage CreateReceivedMessage(string receivedInMessageId, AS4Message as4Message)
        {
            var inMessage = new InMessage(receivedInMessageId);

            return new ReceivedEntityMessage(inMessage, as4Message.ToStream(), as4Message.ContentType);
        }
    }
}