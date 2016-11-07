using System;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.Model;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Submit;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    /// <summary>
    /// Testing <see cref="InternalMessage" />
    /// </summary>
    public class GivenInternalMessageFacts
    {
        private InternalMessage _internalMessage;

        public GivenInternalMessageFacts()
        {
            this._internalMessage = new InternalMessage();
        }

        /// <summary>
        /// Testing the Internal Message
        /// </summary>
        public class GivenValidArgumentsInternalMessage : GivenInternalMessageFacts
        {
            [Fact]
            public void ThenAddAttachmentSucceeds()
            {
                // Arrange
                var memoryStream = new MemoryStream();
                var submitMessage = new SubmitMessage {Payloads = new[] {new Payload(location: string.Empty)}};
                base._internalMessage = new InternalMessage(submitMessage);
                // Act
                base._internalMessage.AddAttachments((Payload payload) => memoryStream);
                // Assert
                Assert.NotNull(base._internalMessage.AS4Message.Attachments);
                Assert.Equal(memoryStream, base._internalMessage.AS4Message.Attachments.First().Content);
            }

            [Fact]
            public void ThenGettingMessageIdsSucceeds()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                var as4Message = new AS4Message();
                as4Message.UserMessages.Add(new UserMessage {MessageId = messageId});
                base._internalMessage = new InternalMessage(as4Message);
                // Act
                string[] messageIds = base._internalMessage.AS4Message.MessageIds;
                // Assert
                Assert.NotNull(messageIds);
                Assert.Equal(messageId, messageIds.First());
            }

            [Fact]
            public void ThenHasAttachmentsIsCorrectTrue()
            {
                // Arrange
                var as4Message = new AS4Message();
                as4Message.Attachments.Add(new Attachment(id: "attachment-id"));
                base._internalMessage = new InternalMessage(as4Message);
                // Act
                bool hasAttachments = base._internalMessage.AS4Message.HasAttachments;
                // Assert
                Assert.True(hasAttachments);
            }

            [Fact]
            public void ThenHasAttachmentsIsCorrectFalse()
            {
                // Arrange
                var as4Message = new AS4Message();
                base._internalMessage = new InternalMessage(as4Message);
                // Act
                bool hasAttachments = base._internalMessage.AS4Message.HasAttachments;
                // Assert
                Assert.False(hasAttachments);
            }

            [Fact]
            public void ThenHasPayloadsIsCorrectTrue()
            {
                // Arrange
                var submitMessage = new SubmitMessage {Payloads = new[] {new Payload(location: string.Empty)}};
                base._internalMessage = new InternalMessage(submitMessage);
                // Act
                bool hasPayloads = base._internalMessage.SubmitMessage.HasPayloads;
                // Assert
                Assert.True(hasPayloads);
            }

            [Fact]
            public void ThenHasPayloadsIsCorrectFalse()
            {
                // Arrange
                var submitMessage = new SubmitMessage();
                base._internalMessage = new InternalMessage(submitMessage);
                // Act
                bool hasPayloads = base._internalMessage.SubmitMessage.HasPayloads;
                // Assert
                Assert.False(hasPayloads);
            }

            [Fact]
            public void ThenNoAttachmentsAreAddedWithZeroPayloads()
            {
                // Arrange
                var submitMessage = new SubmitMessage();
                base._internalMessage = new InternalMessage(submitMessage);
                // Act
                base._internalMessage.AddAttachments((Payload payload) => new MemoryStream());
                // Assert
                Assert.False(base._internalMessage.AS4Message.HasAttachments);
            }

            [Fact]
            public void ThenInternalMessageHasPrefixFromUserMessage()
            {
                // Arrange
                var userMessage = new UserMessage() {MessageId = "message-Id"};
                var as4Message = new AS4Message();
                as4Message.UserMessages.Add(userMessage);
                var internalMessage = new InternalMessage(as4Message);
                // Act
                string prefix = internalMessage.Prefix;
                // Assert
                Assert.Equal($"[{userMessage.MessageId}]", prefix);
            }

            [Fact]
            public void ThenInternalMessageHasPrefixFromSignalMessage()
            {
                // Arrange
                var signalMessage = new SignalMessage() { MessageId = "message-Id" };
                var as4Message = new AS4Message();
                as4Message.SignalMessages.Add(signalMessage);
                var internalMessage = new InternalMessage(as4Message);
                // Act
                string prefix = internalMessage.Prefix;
                // Assert
                Assert.Equal($"[{signalMessage.MessageId}]", prefix);
            }
        }

        /// <summary>
        /// Testing the Internal Message with invalid arguments
        /// </summary>
        public class GivenInvalidArgumentsInternalMessage : GivenInternalMessageFacts
        {
            [Fact]
            public void ThenGettingMessageIdsFailsWitEmptyAS4Message()
            {
                // Arrange
                base._internalMessage = new InternalMessage(new AS4Message());
                // Act
                string[] messageIds = base._internalMessage.AS4Message.MessageIds;
                // Assert
                Assert.NotNull(messageIds);
                Assert.Empty(messageIds);
            }

            [Fact]
            public void ThenGettingMessageIdsFailsWithNullAS4Message()
            {
                // Arrange
                base._internalMessage = new InternalMessage(as4Message: null);
                // Act / Assert
                Assert.Throws<NullReferenceException>(() => base._internalMessage.AS4Message.MessageIds);
            }

            [Fact]
            public void ThenHasAttachmentsFailsWithNullAS4Message()
            {
                // Arrange
                base._internalMessage = new InternalMessage(as4Message: null);
                // Act / Assert
                Assert.Throws<NullReferenceException>(() => base._internalMessage.AS4Message.HasAttachments);
            }
        }
    }
}