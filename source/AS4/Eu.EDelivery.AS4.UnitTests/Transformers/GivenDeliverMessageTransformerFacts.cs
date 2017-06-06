using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
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
            ReceivedMessageEntityMessage receivedMessage = CreateReceivedMessage(
                updateInMessage: m => m.EbmsMessageId = "ignored id",
                as4Message: new AS4Message());

            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(
                () => sut.TransformAsync(receivedMessage, CancellationToken.None));
        }

        [Fact]
        public async Task FailsToTransform_IfInvalidMessageEntityHasGiven()
        {
            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(
                () => new DeliverMessageTransformer().TransformAsync(message: null, cancellationToken: CancellationToken.None));
        }

        [Fact]
        public async Task TransformRemovesUnnecessaryAttachments()
        {
            // Arrange
            const string expectedId = "usermessage-id";
            const string expectedUri = "expected-attachment-uri";
            AS4Message as4Message = new AS4MessageBuilder()
                .WithUserMessage(new FilledUserMessage(expectedId, expectedUri))
                .WithAttachment(FilledAttachment(expectedUri))
                .WithAttachment(FilledAttachment())
                .WithAttachment(FilledAttachment())
                .Build();

            // Act
            MessagingContext actualMessage = await ExerciseTransform(expectedId, as4Message);

            // Assert
            Assert.Equal(1, actualMessage.AS4Message.Attachments.Count);
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

            AS4Message as4Message = new AS4MessageBuilder()
                 .WithUserMessage(new FilledUserMessage(expectedId))
                 .WithUserMessage(new FilledUserMessage())
                 .Build();

            ReceivedMessageEntityMessage receivedMessage = CreateReceivedMessage(m => m.EbmsMessageId = expectedId, as4Message);
            var sut = new DeliverMessageTransformer();

            // Act
            MessagingContext actualMessage = await sut.TransformAsync(receivedMessage, CancellationToken.None);

            // Assert
            Assert.Equal(1, actualMessage.AS4Message.UserMessages.Count);
            UserMessage actualUserMessage = actualMessage.AS4Message.PrimaryUserMessage;
            Assert.Equal(expectedId, actualUserMessage.MessageId);
        }

        private static async Task<MessagingContext> ExerciseTransform(string expectedId, AS4Message as4Message)
        {
            ReceivedMessageEntityMessage receivedMessage = CreateReceivedMessage(m => m.EbmsMessageId = expectedId, as4Message);
            var sut = new DeliverMessageTransformer();

            return await sut.TransformAsync(receivedMessage, CancellationToken.None);
        }

        private static ReceivedMessageEntityMessage CreateReceivedMessage(Action<InMessage> updateInMessage, AS4Message as4Message)
        {
            var inMessage = new InMessage();
            updateInMessage(inMessage);

            return new ReceivedMessageEntityMessage(inMessage)
            {
                RequestStream = as4Message.ToStream(),
                ContentType = as4Message.ContentType
            };
        }
    }
}