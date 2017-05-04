using System;
using System.Collections.Generic;
using System.Threading;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
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
                AS4Message as4Message = CreateDefaultAS4Message(Guid.NewGuid().ToString());

                // Act
                OutMessage outMessage = OutMessageBuilder.ForAS4Message(as4Message.PrimaryUserMessage, as4Message)
                                                         .Build(CancellationToken.None);

                // Assert
                Assert.NotNull(outMessage);
                Assert.Equal(as4Message.ContentType, outMessage.ContentType);
                Assert.Equal(AS4XmlSerializer.ToString(as4Message.SendingPMode), outMessage.PMode);
            }

            [Fact]
            public void ThenBuildOutMessageSucceedsWithAS4MessageAndEbmsMessageId()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                AS4Message as4Message = CreateDefaultAS4Message(messageId);


                // Act
                OutMessage outMessage = OutMessageBuilder.ForAS4Message(as4Message.PrimaryUserMessage, as4Message)
                                                         .Build(CancellationToken.None);

                // Assert
                Assert.Equal(messageId, outMessage.EbmsMessageId);
            }

            [Fact]
            public void ThenBuildOutMessageSucceedsWithAS4MessageAndMessageType()
            {
                // Arrange
                AS4Message as4Message = CreateDefaultAS4Message(Guid.NewGuid().ToString());
                const MessageType messageType = MessageType.Receipt;

                // Act
                OutMessage outMessage = OutMessageBuilder.ForAS4Message(as4Message.PrimaryUserMessage, as4Message)
                                                          .WithEbmsMessageType(messageType)
                                                          .Build(CancellationToken.None);

                // Assert
                Assert.Equal(messageType, outMessage.EbmsMessageType);
            }
        }

        protected AS4Message CreateDefaultAS4Message(string messageId)
        {
            return new AS4Message
            {
                ContentType = "application/soap+xml",
                SendingPMode = new SendingProcessingMode { Id = "pmode-id" },
                UserMessages = new List<UserMessage>() { new UserMessage(messageId) }
            };
        }
    }
}