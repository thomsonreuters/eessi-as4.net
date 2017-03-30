using System;
using System.Collections.Generic;
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
    /// Testing <see cref="InMessageBuilder" />
    /// </summary>
    public class GivenInMessageBuilderFacts
    {
        private readonly Mock<ISerializerProvider> _mockedProvider;

        public GivenInMessageBuilderFacts()
        {
            _mockedProvider = new Mock<ISerializerProvider>();
            _mockedProvider.Setup(p => p.Get(It.IsAny<string>())).Returns(new Mock<ISerializer>().Object);
        }

        public class GivenValidArguments : GivenInMessageBuilderFacts
        {
            [Fact]
            public void ThenBuildInMessageSucceedsWithAS4MessageAndMessageUnit()
            {
                // Arrange
                AS4Message as4Message = CreateDefaultAS4Message();
                MessageUnit messageUnit = CreateDefaultMessageUnit();

                // Act
                InMessage inMessage = new InMessageBuilder(_mockedProvider.Object)
                    .WithAS4Message(as4Message)
                    .WithMessageUnit(messageUnit)
                    .WithPModeString(AS4XmlSerializer.Serialize(as4Message.ReceivingPMode))
                    .Build(CancellationToken.None);

                // Assert
                Assert.NotNull(inMessage);
                Assert.Equal(as4Message.ContentType, inMessage.ContentType);
                Assert.Equal(AS4XmlSerializer.Serialize(as4Message.ReceivingPMode), inMessage.PMode);
            }

            [Fact]
            public void ThenBuildInMessageSucceedsWithAS4MessageAndMessageUnitMessageType()
            {
                // Arrange
                AS4Message as4Message = CreateDefaultAS4Message();
                MessageUnit messageUnit = CreateDefaultMessageUnit();
                const MessageType messageType = MessageType.Receipt;

                // Act
                InMessage inMessage = new InMessageBuilder(_mockedProvider.Object)
                    .WithAS4Message(as4Message)
                    .WithMessageUnit(messageUnit)
                    .WithEbmsMessageType(messageType)
                    .Build(CancellationToken.None);

                // Assert
                Assert.Equal(messageUnit.MessageId, inMessage.EbmsMessageId);
                Assert.Equal(messageUnit.RefToMessageId, inMessage.EbmsRefToMessageId);
                Assert.Equal(messageType, inMessage.EbmsMessageType);
            }
        }

        public class GivenInvalidArguments : GivenInMessageBuilderFacts
        {
            [Fact]
            public void ThenBuildInMessageFailsWitMissingAS4Message()
            {
                // Arrange
                MessageUnit messageUnit = CreateDefaultMessageUnit();
                const MessageType messageType = MessageType.Error;

                // Act / Assert
                Assert.Throws<AS4Exception>(
                    () => new InMessageBuilder()
                        .WithMessageUnit(messageUnit)
                        .WithEbmsMessageType(messageType)
                        .Build(CancellationToken.None));
            }

            [Fact]
            public void ThenBulidInMessageFailsWithMissingMessageUnit()
            {
                // Arrange
                AS4Message as4Message = CreateDefaultAS4Message();
                const MessageType messageType = MessageType.UserMessage;

                // Act / Assert
                Assert.Throws<AS4Exception>(
                    () => new InMessageBuilder()
                        .WithAS4Message(as4Message)
                        .WithEbmsMessageType(messageType)
                        .Build(CancellationToken.None));
            }
        }

        protected MessageUnit CreateDefaultMessageUnit()
        {
            return new Receipt(Guid.NewGuid().ToString()) {RefToMessageId = Guid.NewGuid().ToString()};
        }

        protected AS4Message CreateDefaultAS4Message()
        {
            return new AS4Message
            {
                ContentType = "application/soap+xml",
                SendingPMode = new SendingProcessingMode(),
                Attachments = new List<Attachment>()
            };
        }
    }
}