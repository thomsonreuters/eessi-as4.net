using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.UnitTests.Model;
using Xunit;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Entities
{
    /// <summary>
    /// Testing <see cref="OutMessageBuilder" />
    /// </summary>
    public class GivenOutMessageBuilderFacts
    {
        public class GivenValidArguments : GivenOutMessageBuilderFacts
        {
            [Theory]
            [InlineData(MessageExchangePatternBinding.Push, MessageExchangePattern.Push)]
            [InlineData(MessageExchangePatternBinding.Pull, MessageExchangePattern.Pull)]
            public void BuilderTakesPModeAsMEPBinding(
                MessageExchangePatternBinding pmodeMep,
                MessageExchangePattern expected)
            {
                // Arrange
                var context = new MessagingContext(AS4Message.Empty, MessagingContextMode.Send)
                {
                    SendingPMode = new SendingProcessingMode {MepBinding = pmodeMep}
                };

                // Act
                OutMessage message = OutMessageBuilder.ForMessageUnit(new FilledUserMessage(), context).Build(CancellationToken.None);

                // Assert
                MessageExchangePattern actual = message.MEP;
                Assert.Equal(expected, actual);
            }

            [Fact]
            public async Task ThenBuildOutMessageSucceedsWithAS4Message()
            {
                // Arrange
                AS4Message as4Message = CreateAS4MessageWithUserMessage(Guid.NewGuid().ToString());

                // Act
                OutMessage outMessage = BuildForUserMessage(as4Message);

                // Assert
                Assert.NotNull(outMessage);
                Assert.Equal(as4Message.ContentType, outMessage.ContentType);
                Assert.Equal(MessageType.UserMessage, outMessage.EbmsMessageType);
                Assert.Equal(await AS4XmlSerializer.ToStringAsync(ExpectedPMode()), outMessage.PMode);
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

            private OutMessage BuildForUserMessage(AS4Message as4Message)
            {
                return OutMessageBuilder.ForMessageUnit(as4Message.PrimaryUserMessage, new MessagingContext(as4Message, MessagingContextMode.Unknown) { SendingPMode = ExpectedPMode() })
                                                         .Build(CancellationToken.None);
            }

            [Fact]
            public void ThenBuildOutMessageSucceedsForReceiptMessage()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                AS4Message as4Message = AS4Message.Create(new Receipt(messageId), ExpectedPMode());

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
                AS4Message as4Message = AS4Message.Create(new Error(messageId), ExpectedPMode());

                // Act
                OutMessage outMessage = BuildForSignalMessage(as4Message);

                // Assert
                Assert.Equal(messageId, outMessage.EbmsMessageId);
                Assert.Equal(MessageType.Error, outMessage.EbmsMessageType);
            }

            private static OutMessage BuildForSignalMessage(AS4Message as4Message)
            {           
                return OutMessageBuilder.ForMessageUnit(as4Message.PrimarySignalMessage, new MessagingContext(as4Message, MessagingContextMode.Send))
                                                                        .Build(CancellationToken.None);
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