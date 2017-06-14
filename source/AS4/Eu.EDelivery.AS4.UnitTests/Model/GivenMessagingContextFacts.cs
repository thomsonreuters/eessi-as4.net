﻿using System;
using System.Linq;
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

                var expected = new MessagingContext(AS4MessageWithEbmsMessageId(), MessagingContextMode.Unknown)
                {
                    ReceivingPMode = new ReceivingProcessingMode(),
                    SendingPMode = new SendingProcessingMode()
                };

                // Act
                MessagingContext actual = expected.CloneWith(AS4MessageWithoutEbmsMessageId());

                // Assert
                Assert.NotEqual(expected.AS4Message, actual.AS4Message);
                Assert.Equal(expected.SendingPMode, actual.SendingPMode);
                Assert.Equal(expected.ReceivingPMode, actual.ReceivingPMode);
            }

            private static AS4Message AS4MessageWithEbmsMessageId()
            {
                return AS4Message.Create(new FilledNRRReceipt());
            }

            private static AS4Message AS4MessageWithoutEbmsMessageId()
            {
                return AS4Message.Create(null, contentType: "other");
            }

            [Fact]
            public void ThenGettingMessageIdsSucceeds()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();

                AS4Message as4Message = AS4Message.Create(new UserMessage(messageId));
                var context = new MessagingContext(as4Message, MessagingContextMode.Unknown);

                // Act
                string[] messageIds = context.AS4Message.MessageIds;

                // Assert
                Assert.NotNull(messageIds);
                Assert.Equal(messageId, messageIds.First());
            }

            [Fact]
            public void ThenHasAttachmentsIsCorrectFalse()
            {
                // Arrange
                var context = new MessagingContext(AS4Message.Empty, MessagingContextMode.Unknown);

                // Act
                bool hasAttachments = context.AS4Message.HasAttachments;

                // Assert
                Assert.False(hasAttachments);
            }

            [Fact]
            public void ThenHasAttachmentsIsCorrectTrue()
            {
                // Arrange
                AS4Message as4Message = AS4Message.Create(soapEnvelope: null, contentType: null);
                as4Message.AddAttachment(new Attachment("attachment-id"));
                var context = new MessagingContext(as4Message, MessagingContextMode.Unknown);

                // Act
                bool hasAttachments = context.AS4Message.HasAttachments;

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

                var context = new MessagingContext(AS4Message.Create(signalMessage), MessagingContextMode.Unknown);

                // Act
                string prefix = context.Prefix;

                // Assert
                Assert.Equal($"[{signalMessage.MessageId}]", prefix);
            }

            [Fact]
            public void ThenInternalMessageHasPrefixFromUserMessage()
            {
                // Arrange
                var userMessage = new UserMessage("message-Id");

                AS4Message as4Message = AS4Message.Create(userMessage);
                var context = new MessagingContext(as4Message, MessagingContextMode.Unknown);

                // Act
                string prefix = context.Prefix;

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
                var context = new MessagingContext(AS4Message.Empty, MessagingContextMode.Unknown);

                // Act
                string[] messageIds = context.AS4Message.MessageIds;

                // Assert
                Assert.NotNull(messageIds);
                Assert.Empty(messageIds);
            }

            [Fact]
            public void ThenGettingMessageIdsFailsWithNullAS4Message()
            {
                // Arrange
                var internalMessage = new MessagingContext(as4Message: null, mode: MessagingContextMode.Unknown);

                // Act / Assert
                Assert.Throws<NullReferenceException>(() => internalMessage.AS4Message.MessageIds);
            }

            [Fact]
            public void ThenHasAttachmentsFailsWithNullAS4Message()
            {
                // Arrange
                var internalMessage = new MessagingContext(as4Message: null, mode: MessagingContextMode.Unknown);

                // Act / Assert
                Assert.Throws<NullReferenceException>(() => internalMessage.AS4Message.HasAttachments);
            }
        }
    }
}