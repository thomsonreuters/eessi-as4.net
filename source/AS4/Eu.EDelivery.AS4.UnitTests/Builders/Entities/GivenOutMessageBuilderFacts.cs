using System;
using System.Threading;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Entities
{
    /// <summary>
    /// Testing <see cref="OutMessageBuilder"/>
    /// </summary>
    public class GivenOutMessageBuilderFacts
    {
        private readonly Mock<ISerializerProvider> _mockedProvider;

        public GivenOutMessageBuilderFacts()
        {
            this._mockedProvider = new Mock<ISerializerProvider>();
            this._mockedProvider
                .Setup(p => p.Get(It.IsAny<string>()))
                .Returns(new Mock<ISerializer>().Object);
        }

        public class GivenValidArguments : GivenOutMessageBuilderFacts
        {
            [Fact]
            public void ThenBuildOutMessageSucceedsWithAS4Message()
            {
                // Arrange
                AS4Message as4Message = base.CreateDefaultAS4Message();
                // Act
                OutMessage outMessage = new OutMessageBuilder(base._mockedProvider.Object)
                    .WithAS4Message(as4Message).Build(CancellationToken.None);
                // Assert
                Assert.NotNull(outMessage);
                Assert.Equal(as4Message.ContentType, outMessage.ContentType);
                string xmlPMode = AS4XmlSerializer.Serialize(as4Message.SendingPMode);
                Assert.Equal(xmlPMode, outMessage.PMode);
            }

            [Fact]
            public void ThenBuildOutMessageSucceedsWithAS4MessageAndMessageType()
            {
                // Arrange
                AS4Message as4Message = base.CreateDefaultAS4Message();
                const MessageType messageType = MessageType.Receipt;
                // Act
                OutMessage outMessage = new OutMessageBuilder(base._mockedProvider.Object)
                    .WithAS4Message(as4Message).WithEbmsMessageType(messageType).Build(CancellationToken.None);
                // Assert
                Assert.Equal(messageType, outMessage.EbmsMessageType);
            }

            [Fact]
            public void ThenBuildOutMessageSucceedsWithAS4MessageAndEbmsMessageId()
            {
                // Arrange
                AS4Message as4Message = base.CreateDefaultAS4Message();
                string messageId = Guid.NewGuid().ToString();
                // Act
                OutMessage outMessage = new OutMessageBuilder(base._mockedProvider.Object)
                    .WithAS4Message(as4Message).WithEbmsMessageId(messageId).Build(CancellationToken.None);
                // Assert
                Assert.Equal(messageId, outMessage.EbmsMessageId);
            }
        }

        public class GivenInvalidArguments : GivenOutMessageBuilderFacts
        {
            [Fact]
            public void ThenBuildFailsWithMissingAS4Message()
            {
                // Arrange
                const MessageType messageType = MessageType.Error;
                // Act
                Assert.Throws<AS4Exception>(() => new OutMessageBuilder(base._mockedProvider.Object)
                    .WithEbmsMessageType(messageType).Build(CancellationToken.None));
            }
        }

        protected AS4Message CreateDefaultAS4Message()
        {
            return new AS4Message()
            {
                ContentType = "application/soap+xml",
                SendingPMode = new SendingProcessingMode() { Id = "pmode-id"}
            };
        }
    }
}
