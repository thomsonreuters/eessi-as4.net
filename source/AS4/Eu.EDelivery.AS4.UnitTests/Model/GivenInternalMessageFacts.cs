using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.UnitTests.Common;
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
            _internalMessage = new InternalMessage();
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        /// <summary>
        /// Testing the Internal Message
        /// </summary>
        public class GivenValidArgumentsInternalMessage : GivenInternalMessageFacts
        {
            [Fact]
            public async Task ThenAddAttachmentSucceeds()
            {
                // Arrange
                var memoryStream = new MemoryStream();
                var submitMessage = new SubmitMessage {Payloads = new[] {new Payload(string.Empty)}};
                _internalMessage = new InternalMessage(submitMessage);

                // Act
                await _internalMessage.AddAttachments(async payload => await Task.FromResult(memoryStream));

                // Assert
                Assert.NotNull(_internalMessage.AS4Message.Attachments);
                Assert.Equal(memoryStream, _internalMessage.AS4Message.Attachments.First().Content);
            }

            [Fact]
            public void ThenGettingMessageIdsSucceeds()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                var as4Message = new AS4Message();
                as4Message.UserMessages.Add(new UserMessage(messageId));
                _internalMessage = new InternalMessage(as4Message);

                // Act
                string[] messageIds = _internalMessage.AS4Message.MessageIds;

                // Assert
                Assert.NotNull(messageIds);
                Assert.Equal(messageId, messageIds.First());
            }

            [Fact]
            public void ThenHasAttachmentsIsCorrectFalse()
            {
                // Arrange
                var as4Message = new AS4Message();
                _internalMessage = new InternalMessage(as4Message);

                // Act
                bool hasAttachments = _internalMessage.AS4Message.HasAttachments;

                // Assert
                Assert.False(hasAttachments);
            }

            [Fact]
            public void ThenHasAttachmentsIsCorrectTrue()
            {
                // Arrange
                var as4Message = new AS4Message();
                as4Message.Attachments.Add(new Attachment("attachment-id"));
                _internalMessage = new InternalMessage(as4Message);

                // Act
                bool hasAttachments = _internalMessage.AS4Message.HasAttachments;

                // Assert
                Assert.True(hasAttachments);
            }

            [Fact]
            public void ThenHasPayloadsIsCorrectFalse()
            {
                // Arrange
                var submitMessage = new SubmitMessage();
                _internalMessage = new InternalMessage(submitMessage);

                // Act
                bool hasPayloads = _internalMessage.SubmitMessage.HasPayloads;

                // Assert
                Assert.False(hasPayloads);
            }

            [Fact]
            public void ThenHasPayloadsIsCorrectTrue()
            {
                // Arrange
                var submitMessage = new SubmitMessage {Payloads = new[] {new Payload(string.Empty)}};
                _internalMessage = new InternalMessage(submitMessage);

                // Act
                bool hasPayloads = _internalMessage.SubmitMessage.HasPayloads;

                // Assert
                Assert.True(hasPayloads);
            }

            [Fact]
            public void ThenInternalMessageHasPrefixFromSignalMessage()
            {
                // Arrange
                var signalMessage = new Receipt("message-Id");
                var as4Message = new AS4Message();
                as4Message.SignalMessages.Add(signalMessage);
                var internalMessage = new InternalMessage(as4Message);

                // Act
                string prefix = internalMessage.Prefix;

                // Assert
                Assert.Equal($"[{signalMessage.MessageId}]", prefix);
            }

            [Fact]
            public void ThenInternalMessageHasPrefixFromUserMessage()
            {
                // Arrange
                var userMessage = new UserMessage("message-Id");
                var as4Message = new AS4Message();
                as4Message.UserMessages.Add(userMessage);
                var internalMessage = new InternalMessage(as4Message);

                // Act
                string prefix = internalMessage.Prefix;

                // Assert
                Assert.Equal($"[{userMessage.MessageId}]", prefix);
            }

            [Fact]
            public async Task ThenNoAttachmentsAreAddedWithZeroPayloads()
            {
                // Arrange
                var submitMessage = new SubmitMessage();
                _internalMessage = new InternalMessage(submitMessage);

                // Act
                await _internalMessage.AddAttachments(async payload => await Task.FromResult(new MemoryStream()));

                // Assert
                Assert.False(_internalMessage.AS4Message.HasAttachments);
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
                _internalMessage = new InternalMessage(new AS4Message());

                // Act
                string[] messageIds = _internalMessage.AS4Message.MessageIds;

                // Assert
                Assert.NotNull(messageIds);
                Assert.Empty(messageIds);
            }

            [Fact]
            public void ThenGettingMessageIdsFailsWithNullAS4Message()
            {
                // Arrange
                _internalMessage = new InternalMessage(as4Message: null);

                // Act / Assert
                Assert.Throws<NullReferenceException>(() => _internalMessage.AS4Message.MessageIds);
            }

            [Fact]
            public void ThenHasAttachmentsFailsWithNullAS4Message()
            {
                // Arrange
                _internalMessage = new InternalMessage(as4Message: null);

                // Act / Assert
                Assert.Throws<NullReferenceException>(() => _internalMessage.AS4Message.HasAttachments);
            }
        }
    }
}