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
            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void UsePModeForDefiningMultiHopMessage(bool expected)
            {
                // Arrange
                var multiHopPMode = new SendingProcessingMode {MessagePackaging = {IsMultiHop = expected}};

                // Act
                AS4Message message = new AS4MessageBuilder(multiHopPMode).Build();

                // Assert
                Assert.Equal(expected, message.IsMultiHopMessage);
            }

            [Fact]
            public void ThenBuildMultipleTimesSameMessagesEveryTimeSucceeds()
            {
                // Arrange 
                AS4MessageBuilder builder = _builder.WithUserMessage(new UserMessage("message-id"));

                // Act
                AS4Message firstMessage = builder.Build();
                AS4Message secondMessage = builder.Build();

                // Assert
                Assert.Equal(firstMessage, secondMessage);
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