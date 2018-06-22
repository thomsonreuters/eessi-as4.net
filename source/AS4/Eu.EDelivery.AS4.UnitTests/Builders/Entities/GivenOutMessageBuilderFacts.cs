using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
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
            public async Task ThenBuildOutMessageSucceedsWithAS4Message()
            {
                // Arrange
                AS4Message as4Message = CreateAS4MessageWithUserMessage(Guid.NewGuid().ToString());

                // Act
                OutMessage outMessage = BuildForPrimaryMessageUnit(as4Message);

                // Assert
                Assert.NotNull(outMessage);
                Assert.Equal(as4Message.ContentType, outMessage.ContentType);
                Assert.Equal(MessageType.UserMessage, outMessage.EbmsMessageType.ToEnum<MessageType>());
                Assert.Equal(await AS4XmlSerializer.ToStringAsync(ExpectedPMode()), outMessage.PMode);
            }

            [Fact]
            public void ThenBuildOutMessageSucceedsWithAS4MessageAndEbmsMessageId()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                AS4Message as4Message = CreateAS4MessageWithUserMessage(messageId);


                // Act
                OutMessage outMessage = BuildForPrimaryMessageUnit(as4Message);

                // Assert
                Assert.Equal(messageId, outMessage.EbmsMessageId);
            }

            [Fact]
            public void ThenBuildOutMessageSucceedsForReceiptMessage()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                AS4Message as4Message = AS4Message.Create(new Receipt(messageId, Guid.NewGuid().ToString()), ExpectedPMode());

                // Act
                OutMessage outMessage = BuildForPrimaryMessageUnit(as4Message);

                // Assert
                Assert.Equal(messageId, outMessage.EbmsMessageId);
                Assert.Equal(MessageType.Receipt, outMessage.EbmsMessageType.ToEnum<MessageType>());
            }

            [Fact]
            public void ThenBuildOutMessageSucceedsForErrorMessage()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                AS4Message as4Message = AS4Message.Create(new Error(messageId), ExpectedPMode());

                // Act
                OutMessage outMessage = BuildForPrimaryMessageUnit(as4Message);

                // Assert
                Assert.Equal(messageId, outMessage.EbmsMessageId);
                Assert.Equal(MessageType.Error, outMessage.EbmsMessageType.ToEnum<MessageType>());
            }

            private OutMessage BuildForPrimaryMessageUnit(AS4Message m)
            {
                return OutMessageBuilder
                    .ForMessageUnit(m.PrimaryMessageUnit, m.ContentType, ExpectedPMode())
                    .Build();
            }
        }

        protected SendingProcessingMode ExpectedPMode()
        {
            return new SendingProcessingMode { Id = "pmode-id" };
        }

        protected AS4Message CreateAS4MessageWithUserMessage(string messageId)
        {
            return AS4Message.Create(new UserMessage(messageId), ExpectedPMode());
        }
    }
}