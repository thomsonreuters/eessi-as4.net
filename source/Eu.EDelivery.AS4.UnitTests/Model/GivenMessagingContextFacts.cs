using System;
using System.Linq;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Xunit;
using MessageInfo = Eu.EDelivery.AS4.Model.Common.MessageInfo;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    /// <summary>
    /// Testing <see cref="MessagingContext" />
    /// </summary>
    public class GivenMessagingContextFacts
    {

        /// <summary>
        /// Testing the Internal Message
        /// </summary>
        public class GivenValidArguments : GivenMessagingContextFacts
        {
            [Fact]
            public void OverrideMessageWithNotifyMessage()
            {
                // Arrange
                var filledNotify = new NotifyMessageEnvelope(new AS4.Model.Notify.MessageInfo(), Status.Delivered, new byte[0], "type", typeof(InMessage));

                var context = new MessagingContext(filledNotify)
                {
                    SendingPMode = new SendingProcessingMode(),
                    ReceivingPMode = new ReceivingProcessingMode()
                };

                var anonymousNotify = new NotifyMessageEnvelope(null, default(Status), null, null, typeof(InMessage));

                // Act
                context.ModifyContext(anonymousNotify);

                // Assert
                Assert.Equal(anonymousNotify, context.NotifyMessage);
                Assert.NotNull(context.SendingPMode);
                Assert.NotNull(context.ReceivingPMode);
            }

            [Fact]
            public void OverrideMessageWithDeliverMessage()
            {
                // Arrange
                var filledNotify = new DeliverMessageEnvelope(new MessageInfo(), new byte[0], "type");
                var context = new MessagingContext(filledNotify)
                {
                    SendingPMode = new SendingProcessingMode(),
                    ReceivingPMode = new ReceivingProcessingMode()
                };

                var anonymousDeliver = new DeliverMessageEnvelope(null, null, null);

                // Act
                context.ModifyContext(anonymousDeliver);

                // Assert
                Assert.Equal(anonymousDeliver, context.DeliverMessage);
                Assert.NotNull(context.SendingPMode);
                Assert.NotNull(context.ReceivingPMode);
            }

            [Fact]
            public void OverrideMessageWithAS4Message()
            {
                // Arrange
                var context = new MessagingContext(AS4MessageWithEbmsMessageId(), MessagingContextMode.Unknown)
                {
                    ReceivingPMode = new ReceivingProcessingMode(),
                    SendingPMode = new SendingProcessingMode()
                };

                // Act
                context.ModifyContext(AS4MessageWithoutEbmsMessageId());

                // Assert
                Assert.Equal(AS4MessageWithoutEbmsMessageId(), context.AS4Message);
                Assert.NotNull(context.SendingPMode);
                Assert.NotNull(context.ReceivingPMode);
            }

            private static AS4Message AS4MessageWithEbmsMessageId()
            {
                return AS4Message.Create(new FilledNRReceipt());
            }

            private static AS4Message AS4MessageWithoutEbmsMessageId()
            {
                return AS4Message.Create(pmode: null);
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
                AS4Message as4Message = AS4Message.Create(pmode: null);
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
                var submitMessage = new SubmitMessage { Payloads = new[] { new Payload(string.Empty) } };
                var internalMessage = new MessagingContext(submitMessage);

                // Act
                bool hasPayloads = internalMessage.SubmitMessage.HasPayloads;

                // Assert
                Assert.True(hasPayloads);
            }

            [Fact]
            public void ThenMessagingContextHasEbmsIdFromSignalMessage()
            {
                // Arrange
                var signalMessage = new Receipt(Guid.NewGuid().ToString());

                var context = new MessagingContext(AS4Message.Create(signalMessage), MessagingContextMode.Unknown);

                // Act
                string prefix = context.EbmsMessageId;

                // Assert
                Assert.Equal($"{signalMessage.MessageId}", prefix);
            }

            [Fact]
            public void ThenMessagingContextHasEbmsMessageIdFromUserMessage()
            {
                // Arrange
                var userMessage = new UserMessage("message-Id");

                AS4Message as4Message = AS4Message.Create(userMessage);
                var context = new MessagingContext(as4Message, MessagingContextMode.Unknown);

                // Act
                string prefix = context.EbmsMessageId;

                // Assert
                Assert.Equal($"{userMessage.MessageId}", prefix);
            }

        }
    }
}