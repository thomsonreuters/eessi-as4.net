using System;
using System.Collections.Generic;
using System.Threading;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Entities
{
    /// <summary>
    /// Testing <see cref="OutMessageBuilder" />
    /// </summary>
    public class GivenOutMessageBuilderFacts
    {
        public class GivenValidArguments : GivenOutMessageBuilderFacts
        {
            [Fact]
            public void ThenBuildOutMessageSucceedsWithAS4Message()
            {
                // Arrange
                AS4Message as4Message = CreateAS4MessageWithUserMessage(Guid.NewGuid().ToString());

                // Act
                OutMessage outMessage = BuildForUserMessage(as4Message);

                // Assert
                Assert.NotNull(outMessage);
                Assert.Equal(as4Message.ContentType, outMessage.ContentType);
                Assert.Equal(MessageType.UserMessage, outMessage.EbmsMessageType);
                Assert.Equal(AS4XmlSerializer.ToString(as4Message.SendingPMode), outMessage.PMode);
            }

            [Fact]
            public void ThenBuildOutMessageSucceedsWithAS4MessageAndEbmsMessageId()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                AS4Message as4Message = CreateAS4MessageWithUserMessage(messageId);


                // Act
                OutMessage outMessage = BuildForUserMessage(as4Message);

                // Assert
                Assert.Equal(messageId, outMessage.EbmsMessageId);
            }

            private static OutMessage BuildForUserMessage(AS4Message as4Message)
            {
                return OutMessageBuilder.ForInternalMessage(as4Message.PrimaryUserMessage, new InternalMessage(as4Message))
                                                         .Build(CancellationToken.None);
            }

            [Fact]
            public void ThenBuildOutMessageSucceedsForReceiptMessage()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                AS4Message as4Message = CreateAS4MessageWithReceiptMessage(messageId);

                // Act
                OutMessage outMessage = BuildForSignalMessage(as4Message);

                // Assert
                Assert.Equal(messageId, outMessage.EbmsMessageId);
                Assert.Equal(MessageType.Receipt, outMessage.EbmsMessageType);
            }

            [Fact]
            public void ThenBuildOutMessageSucceedsForErrorMessage()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                AS4Message as4Message = CreateAS4MessageWithErrorMessage(messageId);

                // Act
                OutMessage outMessage = BuildForSignalMessage(as4Message);

                // Assert
                Assert.Equal(messageId, outMessage.EbmsMessageId);
                Assert.Equal(MessageType.Error, outMessage.EbmsMessageType);
            }

            private static OutMessage BuildForSignalMessage(AS4Message as4Message)
            {
                return OutMessageBuilder.ForInternalMessage(as4Message.PrimarySignalMessage, new InternalMessage(as4Message))
                                                                         .Build(CancellationToken.None);
            }
        }

        protected AS4Message CreateAS4MessageWithUserMessage(string messageId)
        {
            return new AS4Message
            {
                ContentType = "application/soap+xml",
                SendingPMode = new SendingProcessingMode { Id = "pmode-id" },
                UserMessages = new List<UserMessage>() { new UserMessage(messageId) }
            };
        }

        protected AS4Message CreateAS4MessageWithReceiptMessage(string messageId)
        {
            return new AS4Message
            {
                ContentType = "application/soap+xml",
                SendingPMode = new SendingProcessingMode { Id = "pmode-id" },
                SignalMessages = new List<SignalMessage>() { new Receipt { MessageId = messageId } }
            };
        }

        protected AS4Message CreateAS4MessageWithErrorMessage(string messageId)
        {
            return new AS4Message
            {
                ContentType = "application/soap+xml",
                SendingPMode = new SendingProcessingMode { Id = "pmode-id" },
                SignalMessages = new List<SignalMessage>() { new Error { MessageId = messageId } }
            };
        }
    }
}