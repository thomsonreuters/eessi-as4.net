using System;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Core
{
    /// <summary>
    /// Testing <seealso cref="AS4MessageBuilder" />
    /// </summary>
    public class GivenAS4MessageBuilderFacts
    {
        private readonly AS4MessageBuilder _builder;

        public GivenAS4MessageBuilderFacts()
        {
            this._builder = new AS4MessageBuilder();
        }

        /// <summary>
        /// Testing if the AS4 Builder succeeds
        /// with valid input data
        /// </summary>
        public class GivenAS4MessageBuilderSucceeds : GivenAS4MessageBuilderFacts
        {
            [Fact]
            public void ThenBuilderBreaksDownTheCollectedInfo()
            {
                // Arrange
                var userMessage = new UserMessage();
                AS4Message as4MessageWithUserMessage = this._builder
                    .WithUserMessage(userMessage)
                    .Build();
                // Act
                AS4Message as4MessageWithoutUserMessage = this._builder
                    .BreakDown()
                    .Build();
                // Assert
                Assert.False(
                    as4MessageWithUserMessage.UserMessages.Count ==
                    as4MessageWithoutUserMessage.UserMessages.Count);
            }

            [Fact]
            public void ThenBuildMultipleTimesUniqueMessagesEveryTimeSucceeds()
            {
                // Arrange 
                AS4MessageBuilder builder = this._builder.WithUserMessage(new UserMessage());
                // Act
                AS4Message firstMessage = builder.Build();
                AS4Message secondMessage = builder.Build();
                // Assert
                Assert.NotEqual(firstMessage, secondMessage);
            }

            [Fact]
            public void ThenBuildSimpleUserMessageSucceeds()
            {
                // Arrange
                var userMessage = new UserMessage();
                // Act
                AS4Message message = this._builder.WithUserMessage(userMessage).Build();
                // Assert
                Assert.NotNull(message);
                Assert.Contains(message.UserMessages, m => m == userMessage);
            }

            [Fact]
            public void ThenBuildUserMessageWithPModeSucceeds()
            {
                // Arrange
                var pmode = new SendingProcessingMode();
                // Act
                AS4Message message = this._builder.WithSendingPMode(pmode).Build();
                // Assert
                Assert.NotNull(message);
                Assert.Same(pmode, message.SendingPMode);
            }
        }

        /// <summary>
        /// Testing if the AS4 Builder Fails
        /// </summary>
        public class GivenAS4MessageBuiderFails : GivenAS4MessageBuilderFacts
        {
            [Fact]
            public void ThenBuildSimpleUserMessageFailsWithNull()
            {
                // Act
                Assert.Throws<ArgumentNullException>(
                    () => this._builder.WithUserMessage(null));
            }

            [Fact]
            public void ThenBuildWithNullAsPModeFails()
            {
                // Act
                Assert.Throws<ArgumentNullException>(
                    () => this._builder.WithSendingPMode(null));
            }
        }
    }
}