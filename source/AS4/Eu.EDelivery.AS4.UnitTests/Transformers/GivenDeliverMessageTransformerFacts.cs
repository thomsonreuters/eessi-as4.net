using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Transformers;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Eu.EDelivery.AS4.UnitTests.Model;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    /// <summary>
    /// Testing <see cref="DeliverMessageTransformer"/>
    /// </summary>
    public class GivenDeliverMessageTransformerFacts
    {
        [Fact]
        public async Task FailsToTransform_IfNoUserMessageCanBeFound()
        {
            // Arrange
            var sut = new DeliverMessageTransformer();
            ReceivedMessageEntityMessage receivedMessage = CreateReceivedMessage(receivedInMessageId: "ignored id", as4Message: AS4Message.Empty);

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

            AS4Message message = AS4Message.Create(new FilledUserMessage(expectedId, expectedUri));
            message.AddAttachment(FilledAttachment(expectedUri));
            message.AddAttachment(FilledAttachment());
            message.AddAttachment(FilledAttachment());

            // Act
            MessagingContext actualMessage = await ExerciseTransform(expectedId, message);

            // Assert
            Assert.Equal(1, actualMessage.AS4Message.Attachments.Count());
        }

        private static Attachment FilledAttachment(string attachmentId = "attachment-id")
        {
            return new Attachment(attachmentId)
            {
                Content = new MemoryStream(Encoding.UTF8.GetBytes("serialize me!")),
                ContentType = "text/plain"
            };
        }

        [Fact]
        public async Task TransformerRemoveUnnecessaryUserMessages_IfMessageIsntReferenced()
        {
            // Arrange
            const string expectedId = "usermessage-id";

            AS4Message as4Message = AS4Message.Create(new FilledUserMessage(expectedId));
            as4Message.AddMessageUnit(new FilledUserMessage());

            ReceivedMessageEntityMessage receivedMessage = CreateReceivedMessage(receivedInMessageId: expectedId, as4Message: as4Message);
            var sut = new DeliverMessageTransformer();

            // Act
            MessagingContext actualMessage = await sut.TransformAsync(receivedMessage);

            // Assert
            Assert.Equal(1, actualMessage.AS4Message.UserMessages.Count());
            UserMessage actualUserMessage = actualMessage.AS4Message.PrimaryUserMessage;
            Assert.Equal(expectedId, actualUserMessage.MessageId);
        }

        private static async Task<MessagingContext> ExerciseTransform(string expectedId, AS4Message as4Message)
        {
            ReceivedMessageEntityMessage receivedMessage = CreateReceivedMessage(receivedInMessageId: expectedId, as4Message: as4Message);
            var sut = new DeliverMessageTransformer();

            return await sut.TransformAsync(receivedMessage);
        }

        private static ReceivedMessageEntityMessage CreateReceivedMessage(string receivedInMessageId, AS4Message as4Message)
        {
            var inMessage = new InMessage(receivedInMessageId);

            return new ReceivedMessageEntityMessage(inMessage, as4Message.ToStream(), as4Message.ContentType);
        }
    }
}