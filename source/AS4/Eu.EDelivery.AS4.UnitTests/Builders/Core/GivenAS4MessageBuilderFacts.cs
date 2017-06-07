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
            _builder = new AS4MessageBuilder();
        }

        /// <summary>
        /// Testing if the AS4 Builder succeeds
        /// with valid input data
        /// </summary>
        public class GivenAS4MessageBuilderSucceeds : GivenAS4MessageBuilderFacts
        {
            [Fact]
            public void UsePModeForDefiningMultiHopMessage()
            {
                // Arrange
                var multiHopPMode = new SendingProcessingMode {MessagePackaging = {IsMultiHop = true}};

                // Act
                AS4Message message = new AS4MessageBuilder(multiHopPMode).Build();

                // Assert
                Assert.True(message.IsMultiHopMessage);
            }

            [Fact]
            public void ThenBuilderBreaksDownTheCollectedInfo()
            {
                // Arrange
                var userMessage = new UserMessage("message-id");
                AS4Message as4MessageWithUserMessage = _builder.WithUserMessage(userMessage).Build();

                // Act
                AS4Message as4MessageWithoutUserMessage = _builder.BreakDown().Build();

                // Assert
                Assert.False(
                    as4MessageWithUserMessage.UserMessages.Count == as4MessageWithoutUserMessage.UserMessages.Count);
            }

            [Fact]
            public void ThenBuildMultipleTimesUniqueMessagesEveryTimeSucceeds()
            {
                // Arrange 
                AS4MessageBuilder builder = _builder.WithUserMessage(new UserMessage("message-id"));

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
                var userMessage = new UserMessage("message-id");

                // Act
                AS4Message message = _builder.WithUserMessage(userMessage).Build();

                // Assert
                Assert.NotNull(message);
                Assert.Contains(message.UserMessages, m => m == userMessage);
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
                Assert.Throws<ArgumentNullException>(() => _builder.WithUserMessage(null));
            }

            [Fact]
            public void Fails_IfInvalidPullRequest()
            {
                Assert.ThrowsAny<Exception>(() => _builder.WithPullRequest(mpc: null));
            }

            [Fact]
            public void Fails_IfInvalidAttachment()
            {
                Assert.ThrowsAny<Exception>(() => _builder.WithAttachment(attachment: null));
            }
        }
    }
}