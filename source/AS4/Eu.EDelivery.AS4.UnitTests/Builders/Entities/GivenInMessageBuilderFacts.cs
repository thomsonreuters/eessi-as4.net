using System;
using System.Collections.Generic;
using System.Threading;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Entities
{
    /// <summary>
    /// Testing <see cref="InMessageBuilder" />
    /// </summary>
    public class GivenInMessageBuilderFacts
    {        
        public class GivenValidArguments : GivenInMessageBuilderFacts
        {
            [Fact]
            public void ThenBuildInMessageSucceedsWithAS4MessageAndMessageUnit()
            {
                // Arrange
                AS4Message as4Message = AS4Message.Empty;
                Receipt receipt = CreateReceiptMessageUnit();

                // Act
                InMessage inMessage = InMessageBuilder.ForSignalMessage(receipt, as4Message)
                                                        .WithPModeString(AS4XmlSerializer.ToString(new ReceivingProcessingMode()))
                                                        .Build(CancellationToken.None);

                // Assert
                Assert.NotNull(inMessage);
                Assert.Equal(as4Message.ContentType, inMessage.ContentType);
                Assert.Equal(AS4XmlSerializer.ToString(new ReceivingProcessingMode()), inMessage.PMode);
                Assert.Equal(MessageType.Receipt, inMessage.EbmsMessageType);
            }
        }

        public class GivenInvalidArguments : GivenInMessageBuilderFacts
        {
            [Fact]
            public void ThenBuildInMessageFailsWitMissingAS4Message()
            {
                // Arrange
                Receipt messageUnit = CreateReceiptMessageUnit();

                // Act / Assert
                Assert.Throws<AS4Exception>(
                    () => InMessageBuilder.ForSignalMessage(messageUnit, belongsToAS4Message: null).Build(CancellationToken.None));
            }

            [Fact]
            public void ThenBulidInMessageFailsWithMissingMessageUnit()
            {
                // Arrange
                AS4Message as4Message = AS4Message.Empty;                

                // Act / Assert
                Assert.Throws<AS4Exception>(
                    () => InMessageBuilder.ForUserMessage(null, as4Message).Build(CancellationToken.None));
            }
        }

        protected Receipt CreateReceiptMessageUnit()
        {
            return new Receipt(Guid.NewGuid().ToString()) { RefToMessageId = Guid.NewGuid().ToString() };
        }
    }
}