using System.Linq;
using Eu.EDelivery.AS4.Model.Core;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    /// <summary>
    /// Testing <see cref="UserMessage"/>
    /// </summary>
    public class GivenUserMessageFacts
    {
        public class GivenValidArguments : GivenUserMessageFacts
        {
            private const string DefaultSenderPartyId = "http://docs.oasis-open.org/ebxml-msg/ebMS/v3.0/ns/core/200704/defaultFrom";
            private const string DefaultReceiverpartyId = "http://docs.oasis-open.org/ebxml-msg/ebMS/v3.0/ns/core/200704/defaultTo";

            [Fact]
            public void ThenUserMessageHasDefaultsPartySender()
            {
                // Act
                var userMessage = new UserMessage(messageId: "message-id");
                // Assert
                Assert.NotNull(userMessage.Sender);
                string firstSenderPartyId = userMessage.Sender.PartyIds.First().Id;
                Assert.Equal(DefaultSenderPartyId, firstSenderPartyId);
            }

            [Fact]
            public void ThenUserMessageHasDefaultsPartyReceiver()
            {
                // Act
                var userMessage = new UserMessage(messageId: "message-id");
                // Assert
                Assert.NotNull(userMessage.Receiver);
                string firstReceiverPartyId = userMessage.Receiver.PartyIds.First().Id;
                Assert.Equal(DefaultReceiverpartyId, firstReceiverPartyId);
            }   

            [Theory, InlineData("message-id")]
            public void ThenUserMessageToStringIsMessageId(string messageId)
            {
                // Arrange
                var userMessage = new UserMessage(messageId: messageId);
                // Act
                string userMessageString = userMessage.ToString();
                // Assert
                Assert.NotNull(userMessage);
                Assert.Equal($"UserMessage [${messageId}]", userMessageString);
            }
        }
    }
}
