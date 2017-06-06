using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;
using MessageInfo = Eu.EDelivery.AS4.Model.Common.MessageInfo;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    /// <summary>
    /// Testing <see cref="MessagingContext" />
    /// </summary>
    public class GivenMessagingContextFacts
    {
        public GivenMessagingContextFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        /// <summary>
        /// Testing the Internal Message
        /// </summary>
        public class GivenValidArguments : GivenMessagingContextFacts
        {
            [Fact]
            public void OverrideMessageWithNotifyMessage()
            {
                // Arrange
                var filledNotify = new NotifyMessageEnvelope(new AS4.Model.Notify.MessageInfo(), Status.Delivered, new byte[0], "type");
                var expected = new MessagingContext(filledNotify)
                {
                    SendingPMode = new SendingProcessingMode(),
                    ReceivingPMode = new ReceivingProcessingMode()
                };
                var anonymousNotify = new NotifyMessageEnvelope(null, default(Status), null, null);

                // Act
                MessagingContext actual = expected.CloneWith(anonymousNotify);

                // Assert
                Assert.NotEqual(expected.NotifyMessage, actual.NotifyMessage);
                Assert.Equal(expected.SendingPMode, actual.SendingPMode);
                Assert.Equal(expected.ReceivingPMode, actual.ReceivingPMode);
            }

            [Fact]
            public void OverrideMessageWithDeliverMessage()
            {
                // Arrange
                var filledNotify = new DeliverMessageEnvelope(new MessageInfo(), new byte[0], "type");
                var expected = new MessagingContext(filledNotify)
                {
                    SendingPMode = new SendingProcessingMode(),
                    ReceivingPMode = new ReceivingProcessingMode()
                };

                var anonymousDeliver = new DeliverMessageEnvelope(null, null, null);

                // Act
                MessagingContext actual = expected.CloneWith(anonymousDeliver);

                // Assert
                Assert.NotEqual(expected.DeliverMessage, actual.DeliverMessage);
                Assert.Equal(expected.SendingPMode, actual.SendingPMode);
                Assert.Equal(expected.ReceivingPMode, actual.ReceivingPMode);
            }

            [Fact]
            public void OverrideMessageWithAS4Message()
            {
                // Arrange
                var expected = new MessagingContext(new AS4Message())
                {
                    ReceivingPMode = new ReceivingProcessingMode(),
                    SendingPMode = new SendingProcessingMode()
                };

                // Act
                MessagingContext actual = expected.CloneWith(new AS4Message {ContentType = "other"});

                // Assert
                Assert.NotEqual(expected.AS4Message, actual.AS4Message);
                Assert.Equal(expected.SendingPMode, actual.SendingPMode);
                Assert.Equal(expected.ReceivingPMode, actual.ReceivingPMode);
            }

            [Fact]
            public void ThenGettingMessageIdsSucceeds()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                var as4Message = new AS4Message();
                as4Message.UserMessages.Add(new UserMessage(messageId));
                var internalMessage = new MessagingContext(as4Message);

                // Act
                string[] messageIds = internalMessage.AS4Message.MessageIds;

                // Assert
                Assert.NotNull(messageIds);
                Assert.Equal(messageId, messageIds.First());
            }

            [Fact]
            public void ThenHasAttachmentsIsCorrectFalse()
            {
                // Arrange
                var internalMessage = new MessagingContext(new AS4Message());

                // Act
                bool hasAttachments = internalMessage.AS4Message.HasAttachments;

                // Assert
                Assert.False(hasAttachments);
            }

            [Fact]
            public void ThenHasAttachmentsIsCorrectTrue()
            {
                // Arrange
                var as4Message = new AS4Message();
                as4Message.Attachments.Add(new Attachment("attachment-id"));
                var internalMessage = new MessagingContext(as4Message);

                // Act
                bool hasAttachments = internalMessage.AS4Message.HasAttachments;

                // Assert
                Assert.True(hasAttachments);
            }

            [Fact]
            public void ThenHasPayloadsIsCorrectFalse()
            {
                // Arrange
                var submitMessage = new SubmitMessage();
                var internalMessage = new MessagingContext(submitMessage);

                // Act
                bool hasPayloads = internalMessage.SubmitMessage.HasPayloads;

                // Assert
                Assert.False(hasPayloads);
            }

            [Fact]
            public void ThenHasPayloadsIsCorrectTrue()
            {
                // Arrange
                var submitMessage = new SubmitMessage {Payloads = new[] {new Payload(string.Empty)}};
                var internalMessage = new MessagingContext(submitMessage);

                // Act
                bool hasPayloads = internalMessage.SubmitMessage.HasPayloads;

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
                var internalMessage = new MessagingContext(as4Message);

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
                var internalMessage = new MessagingContext(as4Message);

                // Act
                string prefix = internalMessage.Prefix;

                // Assert
                Assert.Equal($"[{userMessage.MessageId}]", prefix);
            }
        }

        /// <summary>
        /// Testing the Internal Message with invalid arguments
        /// </summary>
        public class GivenInvalidArguments : GivenMessagingContextFacts
        {
            [Fact]
            public void ThenGettingMessageIdsFailsWitEmptyAS4Message()
            {
                // Arrange
                var internalMessage = new MessagingContext(new AS4Message());

                // Act
                string[] messageIds = internalMessage.AS4Message.MessageIds;

                // Assert
                Assert.NotNull(messageIds);
                Assert.Empty(messageIds);
            }

            [Fact]
            public void ThenGettingMessageIdsFailsWithNullAS4Message()
            {
                // Arrange
                var internalMessage = new MessagingContext(as4Message: null);

                // Act / Assert
                Assert.Throws<NullReferenceException>(() => internalMessage.AS4Message.MessageIds);
            }

            [Fact]
            public void ThenHasAttachmentsFailsWithNullAS4Message()
            {
                // Arrange
                var internalMessage = new MessagingContext(as4Message: null);

                // Act / Assert
                Assert.Throws<NullReferenceException>(() => internalMessage.AS4Message.HasAttachments);
            }
        }
    }
}